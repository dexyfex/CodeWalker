using CodeWalker.GameFiles;

using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Utils
{
    public class Synthesizer : IDisposable
    {
        public const int BufferSize = 0x100;
        public const int SampleRate = 48000;

        public Dat10Synth Synth { get; private set; }
        public float[] Registers { get; private set; }
        public float[][] Buffers { get; private set; }
        public StateBlock[] StateBlocks { get; private set; }
        
        private Dat10Synth.Instruction[] instructions;
        private bool stop;
        private Random rnd = new Random();

        private XAudio2 xAudio2;
        private MasteringVoice masteringVoice;
        private WaveFormat waveFormat;
        private SourceVoice sourceVoice;
        private bool disposed;

        public event EventHandler Stopped;
        public event EventHandler FrameSynthesized;

        public Synthesizer()
        {
            xAudio2 = new XAudio2();
            masteringVoice = new MasteringVoice(xAudio2);
            waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1);
            sourceVoice = new SourceVoice(xAudio2, waveFormat);
            sourceVoice.BufferStart += _ => SubmitNextFrame();
        }

        public void Dispose()
        {
            if (disposed) return;

            sourceVoice.DestroyVoice();
            sourceVoice.Dispose();
            masteringVoice.Dispose();
            xAudio2.Dispose();

            disposed = true;
        }

        public void Play(Dat10Synth synth)
        {
            Synth = synth;
            instructions = Dat10Synth.DisassembleGetInstructions(synth.Bytecode, synth.Constants, synth.Variables);
            Registers = new float[synth.RegistersCount];
            Buffers = new float[synth.BuffersCount][];
            StateBlocks = new StateBlock[synth.StateBlocksCount];
            stop = false;

            for (int i = 0; i < synth.BuffersCount; i++)
                Buffers[i] = new float[BufferSize];

            sourceVoice.FlushSourceBuffers();
            sourceVoice.SetOutputMatrix(1, 2, new[] { 1.0f, 1.0f });
            sourceVoice.SetVolume(1.0f);

            SubmitNextFrame();
            sourceVoice.Start();
        }

        public void Stop()
        {
            stop = true;
        }

        private void SubmitNextFrame()
        {
            if (stop)
            {
                Stopped?.Invoke(this, EventArgs.Empty);
                return;
            }

            SynthesizeFrame();

            var bufferIndex = Synth.OutputsIndices[0];
            if ((bufferIndex < 0) || (bufferIndex >= Buffers.Length)) bufferIndex = 0;

            var audioDataStream = DataStream.Create(Buffers[bufferIndex], true, false);
            var audioBuffer = new AudioBuffer
            {
                Stream = audioDataStream,
                AudioBytes = (int)audioDataStream.Length,
                Flags = BufferFlags.EndOfStream,
            };
            sourceVoice.SubmitSourceBuffer(audioBuffer, null);
        }

        private void SynthesizeFrame()
        {
            for (int i = 0; i < Registers.Length; i++)
                Registers[i] = 0.0f;

            for (int i = 0; i < Buffers.Length; i++)
                for (int k = 0; k < BufferSize; k++)
                    Buffers[i][k] = 0.0f;

            bool frameFinished = false;
            for (int insti = 0; insti < instructions.Length; insti++)
            {
                var inst = instructions[insti];
                if (frameFinished)
                    break;

                var param = inst.Parameters;
                float[] a, b;
                int stateBlock;
                float scalar;
                switch (inst.Opcode)
                {
                    case Dat10Synth.Opcode.COPY_BUFFER:
                        var srcBuffer = GetBuffer(param[0]);
                        for (int i = 0; i < inst.NumberOfOutputs - 1; i++)
                            Array.Copy(srcBuffer, GetBuffer(param[1 + i]), BufferSize);
                        break;
                    case Dat10Synth.Opcode.COPY_REGISTER:
                        var srcRegister = GetRegister(param[0]);
                        for (int i = 0; i < inst.NumberOfOutputs - 1; i++)
                            SetRegister(param[1 + i], srcRegister);
                        break;
                    case Dat10Synth.Opcode.CONVERT_BUFFER_TO_DENORMALIZED:
                        a = GetBuffer(param[0]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = ConvertToDenormalized(a[i]);
                        break;
                    case Dat10Synth.Opcode.CONVERT_SCALAR_TO_DENORMALIZED:
                        SetRegister(param[0], ConvertToDenormalized(GetScalar(param[1])));
                        break;
                    case Dat10Synth.Opcode.CONVERT_BUFFER_TO_NORMALIZED:
                        a = GetBuffer(param[0]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = ConvertToNormalized(a[i]);
                        break;
                    case Dat10Synth.Opcode.CONVERT_SCALAR_TO_NORMALIZED:
                        SetRegister(param[0], ConvertToNormalized(GetScalar(param[1])));
                        break;
                    case Dat10Synth.Opcode.FIRST_OF_BUFFER:
                        SetRegister(param[0], GetBuffer(param[1])[0]);
                        break;
                    case Dat10Synth.Opcode.MULTIPLY_BUFFER_BUFFER:
                        a = GetBuffer(param[0]);
                        b = GetBuffer(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] *= b[i];
                        break;
                    case Dat10Synth.Opcode.MULTIPLY_BUFFER_SCALAR:
                        a = GetBuffer(param[0]);
                        scalar = GetScalar(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] *= scalar;
                        break;
                    case Dat10Synth.Opcode.MULTIPLY_SCALAR_SCALAR:
                        SetRegister(param[0], GetScalar(param[1]) * GetScalar(param[2]));
                        break;
                    case Dat10Synth.Opcode.SUM_BUFFER_BUFFER:
                        a = GetBuffer(param[0]);
                        b = GetBuffer(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] += b[i];
                        break;
                    case Dat10Synth.Opcode.SUM_BUFFER_SCALAR:
                        a = GetBuffer(param[0]);
                        scalar = GetScalar(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] += scalar;
                        break;
                    case Dat10Synth.Opcode.SUM_SCALAR_SCALAR:
                        SetRegister(param[0], GetScalar(param[1]) + GetScalar(param[2]));
                        break;
                    case Dat10Synth.Opcode.SUBTRACT_BUFFER_BUFFER:
                        a = GetBuffer(param[0]);
                        b = GetBuffer(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] -= b[i];
                        break;
                    case Dat10Synth.Opcode.SUBTRACT_BUFFER_SCALAR:
                        a = GetBuffer(param[0]);
                        scalar = GetScalar(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] -= scalar;
                        break;
                    case Dat10Synth.Opcode.SUBTRACT_SCALAR_BUFFER:
                        a = GetBuffer(param[0]);
                        scalar = GetScalar(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = scalar - a[i];
                        break;
                    case Dat10Synth.Opcode.SUBTRACT_SCALAR_SCALAR:
                        SetRegister(param[0], GetScalar(param[1]) - GetScalar(param[2]));
                        break;
                    case Dat10Synth.Opcode.DIVIDE_BUFFER_BUFFER:
                        a = GetBuffer(param[0]);
                        b = GetBuffer(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] /= b[i];
                        break;
                    case Dat10Synth.Opcode.DIVIDE_BUFFER_SCALAR:
                        a = GetBuffer(param[0]);
                        scalar = GetScalar(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] /= scalar;
                        break;
                    case Dat10Synth.Opcode.DIVIDE_SCALAR_SCALAR:
                        SetRegister(param[0], GetScalar(param[1]) / GetScalar(param[2]));
                        break;
                    case Dat10Synth.Opcode.RESCALE_BUFFER_BUFFER:
                        {
                            a = GetBuffer(param[0]);
                            var origMin = GetBuffer(param[1]);
                            var origMax = GetBuffer(param[2]);
                            var newMin = GetBuffer(param[3]);
                            var newMax = GetBuffer(param[4]);

                            for (int i = 0; i < BufferSize; i++)
                                a[i] = Rescale(a[i], origMin[i], origMax[i], newMin[i], newMax[i]);
                        }
                        break;
                    case Dat10Synth.Opcode.RESCALE_BUFFER_SCALAR:
                        {
                            a = GetBuffer(param[0]);
                            var origMin = GetScalar(param[1]);
                            var origMax = GetScalar(param[2]);
                            var newMin = GetScalar(param[3]);
                            var newMax = GetScalar(param[4]);

                            for (int i = 0; i < BufferSize; i++)
                                a[i] = Rescale(a[i], origMin, origMax, newMin, newMax);
                        }
                        break;
                    case Dat10Synth.Opcode.RESCALE_SCALAR_SCALAR:
                        {
                            var value = GetScalar(param[1]);
                            var origMin = GetScalar(param[2]);
                            var origMax = GetScalar(param[3]);
                            var newMin = GetScalar(param[4]);
                            var newMax = GetScalar(param[5]);

                            SetRegister(param[0], Rescale(value, origMin, origMax, newMin, newMax));
                        }
                        break;
                    case Dat10Synth.Opcode.HARD_KNEE_BUFFER:
                        a = GetBuffer(param[0]);
                        if (param[1].Type == Dat10Synth.ParameterType.InputBuffer)
                        {
                            // buffer hardKnee buffer
                            b = GetBuffer(param[1]);
                            for (int i = 0; i < BufferSize; i++)
                                a[i] = HardKnee(a[i], b[i]);
                        }
                        else
                        {
                            // buffer hardKnee scalar
                            scalar = GetScalar(param[1]);
                            for (int i = 0; i < BufferSize; i++)
                                a[i] = HardKnee(a[i], scalar);
                        }
                        break;
                    case Dat10Synth.Opcode.HARD_KNEE_SCALAR_SCALAR:
                        SetRegister(param[0], HardKnee(GetScalar(param[1]), GetScalar(param[2])));
                        break;
                    case Dat10Synth.Opcode.NOISE:
                        a = GetBuffer(param[0]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = (float)rnd.NextDouble() * 2.0f - 1.0f; // TODO(alexguirre): verify that the game generates numbers between [-1.0f, 1.0f]
                        break;
                    case Dat10Synth.Opcode.RANDOM:
                        {
                            var next = GetScalar(param[1]) >= 1.0f;
                            var min = GetScalar(param[2]);
                            var max = GetScalar(param[3]);
                            stateBlock = param[4].Value;

                            float randValue;
                            if (StateBlocks[stateBlock].Y == 0.0f || next)
                            {
                                // first time executing or a new value is needed
                                randValue = rnd.NextFloat(min, max);
                                StateBlocks[stateBlock].X = randValue;
                                StateBlocks[stateBlock].Y = 1.0f;
                            }
                            else
                            {
                                // use previous value
                                randValue = StateBlocks[stateBlock].X;
                            }
                            SetRegister(param[0], randValue);
                        }
                        break;
                    case Dat10Synth.Opcode.ABS_BUFFER:
                        a = GetBuffer(param[0]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = Math.Abs(a[i]);
                        break;
                    case Dat10Synth.Opcode.ABS_SCALAR:
                        SetRegister(param[0], Math.Abs(GetScalar(param[1])));
                        break;
                    case Dat10Synth.Opcode.FLOOR_BUFFER:
                        a = GetBuffer(param[0]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = (float)Math.Floor(a[i]);
                        break;
                    case Dat10Synth.Opcode.FLOOR_SCALAR:
                        SetRegister(param[0], (float)Math.Floor(GetScalar(param[1])));
                        break;
                    case Dat10Synth.Opcode.CEIL_BUFFER:
                        a = GetBuffer(param[0]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = (float)Math.Ceiling(a[i]);
                        break;
                    case Dat10Synth.Opcode.CEIL_SCALAR:
                        SetRegister(param[0], (float)Math.Ceiling(GetScalar(param[1])));
                        break;
                    case Dat10Synth.Opcode.ROUND_BUFFER:
                        a = GetBuffer(param[0]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = (float)Math.Round(a[i]);
                        break;
                    case Dat10Synth.Opcode.ROUND_SCALAR:
                        SetRegister(param[0], (float)Math.Round(GetScalar(param[1])));
                        break;
                    case Dat10Synth.Opcode.SIGN_BUFFER:
                        a = GetBuffer(param[0]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = Math.Sign(a[i]);
                        break;
                    case Dat10Synth.Opcode.SIGN_SCALAR:
                        SetRegister(param[0], Math.Sign(GetScalar(param[1])));
                        break;
                    case Dat10Synth.Opcode.MODULO_BUFFER:
                        a = GetBuffer(param[0]);
                        if (param[1].Type == Dat10Synth.ParameterType.InputBuffer)
                        {
                            // buffer mod buffer
                            b = GetBuffer(param[1]);
                            for (int i = 0; i < BufferSize; i++)
                                a[i] = Modulo(a[i], b[i]);
                        }
                        else
                        {
                            // buffer mod scalar
                            scalar = GetScalar(param[1]);
                            for (int i = 0; i < BufferSize; i++)
                                a[i] = Modulo(a[i], scalar);
                        }
                        break;
                    case Dat10Synth.Opcode.MODULO_SCALAR:
                        SetRegister(param[0], Modulo(GetScalar(param[1]), GetScalar(param[2])));
                        break;
                    case Dat10Synth.Opcode.POW_SCALAR:
                        SetRegister(param[0], Pow(GetScalar(param[1]), GetScalar(param[2])));
                        break;
                    case Dat10Synth.Opcode.POW_BUFFER:
                        a = GetBuffer(param[0]);
                        if (param[1].Type == Dat10Synth.ParameterType.InputBuffer)
                        {
                            // buffer pow buffer
                            b = GetBuffer(param[1]);
                            for (int i = 0; i < BufferSize; i++)
                                a[i] = Pow(a[i], b[i]);
                        }
                        else
                        {
                            // buffer pow scalar
                            scalar = GetScalar(param[1]);
                            for (int i = 0; i < BufferSize; i++)
                                a[i] = Pow(a[i], scalar);
                        }
                        break;
                    case Dat10Synth.Opcode.MAX_SCALAR:
                        {
                            scalar = GetScalar(param[1]);
                            var resetToZero = GetScalar(param[2]) >= 1.0f;
                            stateBlock = param[3].Value;

                            var max = resetToZero ? 0.0f : StateBlocks[stateBlock].X;
                            max = Math.Max(max, Math.Abs(scalar));
                            StateBlocks[stateBlock].X = max;
                            SetRegister(param[0], max);
                        }
                        break;
                    case Dat10Synth.Opcode.MAX_BUFFER:
                        {
                            a = GetBuffer(param[0]);
                            var resetToZero = GetScalar(param[1]) >= 1.0f;
                            stateBlock = param[2].Value;

                            var max = resetToZero ? 0.0f : StateBlocks[stateBlock].X;
                            for (int i = 0; i < BufferSize; i++)
                            {
                                max = Math.Max(max, Math.Abs(a[i]));
                                a[i] = max;
                            }
                            StateBlocks[stateBlock].X = max;
                        }
                        break;
                    case Dat10Synth.Opcode.COMPRESS_BUFFER:
                        Compress(
                            GetBuffer(param[0]),
                            GetScalar(param[1]),
                            GetScalar(param[2]),
                            GetScalar(param[3]),
                            GetScalar(param[4]),
                            ref StateBlocks[param[5].Value]);
                        break;
                    //case Dat10Synth.Opcode._UNUSED_2C:
                    //    break;
                    case Dat10Synth.Opcode.LERP_BUFFER:
                        {
                            a = GetBuffer(param[0]);
                            var min = GetScalar(param[1]);
                            var max = GetScalar(param[2]);
                            for (int i = 0; i < BufferSize; i++)
                                a[i] = Lerp(a[i], min, max);
                        }
                        break;
                    case Dat10Synth.Opcode.LERP_BUFFER_2: // TODO: some better name for LERP_BUFFER_2
                        {
                            var t = GetScalar(param[1]);
                            float[] min, max;
                            if ((param[0].Value & 0xFF) == (param[2].Value & 0xFF))
                            {
                                min = GetBuffer(param[2]);
                                max = GetBuffer(param[3]);
                            }
                            else
                            {
                                min = GetBuffer(param[3]);
                                max = GetBuffer(param[2]);
                            }

                            for (int i = 0; i < BufferSize; i++)
                                min[i] = Lerp(t, min[i], max[i]);
                        }
                        break;
                    case Dat10Synth.Opcode.LERP_SCALAR:
                        {
                            var t = GetScalar(param[1]);
                            var min = GetScalar(param[2]);
                            var max = GetScalar(param[3]);
                            SetRegister(param[0], Lerp(t, min, max));
                        }
                        break;
                    case Dat10Synth.Opcode.HARD_CLIP_BUFFER_BUFFER:
                        a = GetBuffer(param[0]);
                        b = GetBuffer(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = HardClip(a[i], b[i]);
                        break;
                    case Dat10Synth.Opcode.HARD_CLIP_BUFFER_SCALAR:
                        a = GetBuffer(param[0]);
                        scalar = GetScalar(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = HardClip(a[i], scalar);
                        break;
                    case Dat10Synth.Opcode.HARD_CLIP_SCALAR_SCALAR:
                        SetRegister(param[0], HardClip(GetScalar(param[1]), GetScalar(param[2])));
                        break;
                    case Dat10Synth.Opcode.SOFT_CLIP_BUFFER_BUFFER:
                        a = GetBuffer(param[0]);
                        b = GetBuffer(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = SoftClip(a[i], b[i]);
                        break;
                    case Dat10Synth.Opcode.SOFT_CLIP_BUFFER_SCALAR:
                        a = GetBuffer(param[0]);
                        scalar = GetScalar(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = SoftClip(a[i], scalar);
                        break;
                    case Dat10Synth.Opcode.SOFT_CLIP_SCALAR_SCALAR:
                        SetRegister(param[0], SoftClip(GetScalar(param[1]), GetScalar(param[2])));
                        break;
                    case Dat10Synth.Opcode.ENVELOPE_FOLLOWER_BUFFER:
                        {
                            a = GetBuffer(param[0]);
                            var scalar2 = GetScalar(param[1]);
                            var scalar3 = GetScalar(param[2]);
                            stateBlock = param[3].Value;

                            EnvelopeFollower(a, scalar2, scalar3, ref StateBlocks[stateBlock]);
                        }
                        break;
                    case Dat10Synth.Opcode.ENVELOPE_FOLLOWER_SCALAR:
                        {
                            var scalar1 = GetScalar(param[1]);
                            var scalar2 = GetScalar(param[2]);
                            var scalar3 = GetScalar(param[3]);
                            stateBlock = param[4].Value;

                            SetRegister(param[0], EnvelopeFollower(scalar1, scalar2, scalar3, ref StateBlocks[stateBlock]));
                        }
                        break;
                    case Dat10Synth.Opcode.BiquadCoefficients_LowPass_1:
                    case Dat10Synth.Opcode.BiquadCoefficients_LowPass_2:
                        BiquadCoefficientsLowPass(
                            unkFlag: inst.Opcode == Dat10Synth.Opcode.BiquadCoefficients_LowPass_2,
                            GetScalar(param[5]),
                            GetScalar(param[6]),
                            out Registers[param[0].Value & 0xFF],
                            out Registers[param[1].Value & 0xFF],
                            out Registers[param[2].Value & 0xFF],
                            out Registers[param[3].Value & 0xFF],
                            out Registers[param[4].Value & 0xFF]);
                        break;
                    case Dat10Synth.Opcode.BiquadCoefficients_HighPass_1:
                    case Dat10Synth.Opcode.BiquadCoefficients_HighPass_2:
                        BiquadCoefficientsHighPass(
                            unkFlag: inst.Opcode == Dat10Synth.Opcode.BiquadCoefficients_HighPass_2,
                            GetScalar(param[5]),
                            GetScalar(param[6]),
                            out Registers[param[0].Value & 0xFF],
                            out Registers[param[1].Value & 0xFF],
                            out Registers[param[2].Value & 0xFF],
                            out Registers[param[3].Value & 0xFF],
                            out Registers[param[4].Value & 0xFF]);
                        break;
                    case Dat10Synth.Opcode.BiquadCoefficients_BandPass:
                        BiquadCoefficientsBandPass(
                            GetScalar(param[5]),
                            GetScalar(param[6]),
                            out Registers[param[0].Value & 0xFF],
                            out Registers[param[1].Value & 0xFF],
                            out Registers[param[2].Value & 0xFF],
                            out Registers[param[3].Value & 0xFF],
                            out Registers[param[4].Value & 0xFF]);
                        break;
                    case Dat10Synth.Opcode.BiquadCoefficients_BandStop:
                        BiquadCoefficientsBandStop(
                            GetScalar(param[5]),
                            GetScalar(param[6]),
                            out Registers[param[0].Value & 0xFF],
                            out Registers[param[1].Value & 0xFF],
                            out Registers[param[2].Value & 0xFF],
                            out Registers[param[3].Value & 0xFF],
                            out Registers[param[4].Value & 0xFF]);
                        break;
                    case Dat10Synth.Opcode.BiquadCoefficients_PeakingEQ:
                        BiquadCoefficientsPeakingEQ(
                            GetScalar(param[5]),
                            GetScalar(param[6]),
                            GetScalar(param[7]),
                            out Registers[param[0].Value & 0xFF],
                            out Registers[param[1].Value & 0xFF],
                            out Registers[param[2].Value & 0xFF],
                            out Registers[param[3].Value & 0xFF],
                            out Registers[param[4].Value & 0xFF]);
                        break;
                    case Dat10Synth.Opcode.BiquadProcess_2Pole:
                        BiquadFilter2Pole(
                            GetBuffer(param[0]),
                            GetScalar(param[1]),
                            GetScalar(param[2]),
                            GetScalar(param[3]),
                            GetScalar(param[4]),
                            GetScalar(param[5]),
                            ref StateBlocks[param[6].Value]);
                        break;
                    case Dat10Synth.Opcode.BiquadProcess_4Pole:
                        BiquadFilter4Pole(
                            GetBuffer(param[0]),
                            GetScalar(param[1]),
                            GetScalar(param[2]),
                            GetScalar(param[3]),
                            GetScalar(param[4]),
                            GetScalar(param[5]),
                            ref StateBlocks[param[6].Value]);
                        break;
                    case Dat10Synth.Opcode.OnePole_LPF_BUFFER_BUFFER:
                        OnePoleLPF(GetBuffer(param[0]), GetBuffer(param[1]), ref StateBlocks[param[2].Value]);
                        break;
                    case Dat10Synth.Opcode.OnePole_LPF_BUFFER_SCALAR:
                        OnePoleLPF(GetBuffer(param[0]), GetScalar(param[1]), ref StateBlocks[param[2].Value]);
                        break;
                    case Dat10Synth.Opcode.OnePole_LPF_SCALAR_SCALAR:
                        SetRegister(param[0], OnePoleLPF(GetScalar(param[1]), GetScalar(param[2]), ref StateBlocks[param[3].Value]));
                        break;
                    case Dat10Synth.Opcode.OnePole_HPF_BUFFER_BUFFER:
                        OnePoleHPF(GetBuffer(param[0]), GetBuffer(param[1]), ref StateBlocks[param[2].Value]);
                        break;
                    case Dat10Synth.Opcode.OnePole_HPF_BUFFER_SCALAR:
                        OnePoleHPF(GetBuffer(param[0]), GetScalar(param[1]), ref StateBlocks[param[2].Value]);
                        break;
                    case Dat10Synth.Opcode.OnePole_HPF_SCALAR_SCALAR:
                        SetRegister(param[0], OnePoleHPF(GetScalar(param[1]), GetScalar(param[2]), ref StateBlocks[param[3].Value]));
                        break;
                    case Dat10Synth.Opcode.OSC_RAMP_BUFFER_BUFFER:
                        OscillatorRamp(GetBuffer(param[0]), ref StateBlocks[param[1].Value]);
                        break;
                    case Dat10Synth.Opcode.OSC_RAMP_BUFFER_SCALAR:
                        OscillatorRamp(GetBuffer(param[0]), GetScalar(param[1]), ref StateBlocks[param[2].Value]);
                        break;
                    case Dat10Synth.Opcode.OSC_RAMP_SCALAR:
                        SetRegister(param[0], OscillatorRamp(GetScalar(param[1]), ref StateBlocks[param[2].Value]));
                        break;
                    case Dat10Synth.Opcode.SINE_BUFFER:
                        a = GetBuffer(param[0]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = Sine(a[i]);
                        break;
                    case Dat10Synth.Opcode.SINE:
                        SetRegister(param[0], Sine(GetScalar(param[1])));
                        break;
                    case Dat10Synth.Opcode.COSINE_BUFFER:
                        a = GetBuffer(param[0]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = Cosine(a[i]);
                        break;
                    case Dat10Synth.Opcode.COSINE:
                        SetRegister(param[0], Cosine(GetScalar(param[1])));
                        break;
                    case Dat10Synth.Opcode.TRIANGLE_BUFFER:
                        a = GetBuffer(param[0]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = Triangle(a[i]);
                        break;
                    case Dat10Synth.Opcode.TRIANGLE:
                        SetRegister(param[0], Triangle(GetScalar(param[1])));
                        break;
                    case Dat10Synth.Opcode.SQUARE_BUFFER:
                        a = GetBuffer(param[0]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = Square(a[i]);
                        break;
                    case Dat10Synth.Opcode.SQUARE:
                        SetRegister(param[0], Square(GetScalar(param[1])));
                        break;
                    case Dat10Synth.Opcode.SAW_BUFFER:
                        a = GetBuffer(param[0]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = Saw(a[i]);
                        break;
                    case Dat10Synth.Opcode.SAW:
                        SetRegister(param[0], Saw(GetScalar(param[1])));
                        break;
                    case Dat10Synth.Opcode.TRIGGER_LATCH:
                        stateBlock = param[3].Value;
                        var latch = 0.0f;
                        if (param[1].Type == Dat10Synth.ParameterType.InputScalar)
                        {
                            // scalar triggerLatch scalar
                            var value = GetScalar(param[1]);
                            var triggerLimit = GetScalar(param[2]);
                            latch = TriggerLatch(value, triggerLimit, ref StateBlocks[stateBlock]);
                        }
                        else
                        {
                            a = GetBuffer(param[1]); // input buffer
                            if (param[2].Type == Dat10Synth.ParameterType.InputScalar)
                            {
                                // buffer triggerLatch scalar
                                scalar = GetScalar(param[2]);
                                latch = TriggerLatch(a, scalar, ref StateBlocks[stateBlock]);
                            }
                            else
                            {
                                // buffer triggerLatch buffer
                                b = GetBuffer(param[2]);
                                latch = TriggerLatch(a, b, ref StateBlocks[stateBlock]);
                            }
                        }
                        SetRegister(param[0], latch);
                        break;
                    case Dat10Synth.Opcode.ENVELOPE_GEN__R_LINEAR_T_INTERRUPTIBLE:
                    case Dat10Synth.Opcode.ENVELOPE_GEN__R_LINEAR_T_ONE_SHOT:
                    case Dat10Synth.Opcode.ENVELOPE_GEN__R_LINEAR_T_RETRIGGER:
                    case Dat10Synth.Opcode.ENVELOPE_GEN__R_EXP_T_INTERRUPTIBLE:
                    case Dat10Synth.Opcode.ENVELOPE_GEN__R_EXP_T_ONE_SHOT:
                    case Dat10Synth.Opcode.ENVELOPE_GEN__R_EXP_T_RETRIGGER:
                        {
                            // these opcodes can have 1 or 2 outputs, first one is the output buffer and the second optional one is a register set to 1 if the envelope is finished
                            var inputsStart = inst.NumberOfOutputs;
                            var buffer = GetBuffer(param[0]);
                            var predelay = GetScalar(param[inputsStart + 0]);
                            var attack = GetScalar(param[inputsStart + 1]);
                            var decay = GetScalar(param[inputsStart + 2]);
                            var sustain = GetScalar(param[inputsStart + 3]);
                            var hold = GetScalar(param[inputsStart + 4]);
                            var release = GetScalar(param[inputsStart + 5]);
                            var trigger = GetScalar(param[inputsStart + 6]);
                            stateBlock = param[inputsStart + 7].Value;
                            var envFinished = EnvelopeGen(inst.Opcode, buffer, ref StateBlocks[stateBlock], predelay, attack, decay, sustain, hold, release, trigger);
                            if (inst.NumberOfOutputs == 2)
                                SetRegister(param[1], envFinished);
                        }
                        break;
                    case Dat10Synth.Opcode.TIMED_TRIGGER__T_INTERRUPTIBLE:
                    case Dat10Synth.Opcode.TIMED_TRIGGER__T_ONE_SHOT:
                    case Dat10Synth.Opcode.TIMED_TRIGGER__T_RETRIGGER:
                        {
                            var trigger = GetScalar(param[5]);
                            var predelay = GetScalar(param[6]);
                            var attack = GetScalar(param[7]);
                            var decay = GetScalar(param[8]);
                            var hold = GetScalar(param[9]);
                            var release = GetScalar(param[10]);
                            stateBlock = param[11].Value;
                            var result = TimedTrigger(inst.Opcode, ref StateBlocks[stateBlock], trigger, predelay, attack, decay, hold, release);
                            SetRegister(param[0], result.Finished);
                            SetRegister(param[1], result.AttackActive);
                            SetRegister(param[2], result.DecayActive);
                            SetRegister(param[3], result.HoldActive);
                            SetRegister(param[4], result.ReleaseActive);
                        }
                        break;
                    case Dat10Synth.Opcode.READ_VARIABLE:
                        if ((param[1].Value & 0xFF) < Synth.Variables.Length)
                        {
                            SetRegister(param[0], Synth.Variables[param[1].Value & 0xFF]?.Value ?? 0);
                        }
                        break;
                    case Dat10Synth.Opcode.STOP:
                        stop = GetScalar(param[0]) >= 1.0f;
                        break;
                    case Dat10Synth.Opcode.READ_INPUT_BUFFER0:
                    case Dat10Synth.Opcode.READ_INPUT_BUFFER1:
                    case Dat10Synth.Opcode.READ_INPUT_BUFFER2:
                    case Dat10Synth.Opcode.READ_INPUT_BUFFER3:
                    case Dat10Synth.Opcode.READ_INPUT_BUFFER4:
                    case Dat10Synth.Opcode.READ_INPUT_BUFFER5:
                    case Dat10Synth.Opcode.READ_INPUT_BUFFER6:
                    case Dat10Synth.Opcode.READ_INPUT_BUFFER7:
                        throw new NotImplementedException("READ_INPUT_BUFFER* opcodes are not supported");
                    case Dat10Synth.Opcode.NOTE_TO_FREQUENCY_SCALAR:
                        SetRegister(param[0], NoteToFrequency(GetScalar(param[1])));
                        break;
                    case Dat10Synth.Opcode.NOTE_TO_FREQUENCY_BUFFER:
                        a = GetBuffer(param[0]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = NoteToFrequency(a[i]);
                        break;
                    case Dat10Synth.Opcode.SAMPLE_AND_HOLD:
                        {
                            var value = GetScalar(param[1]);
                            var trigger = GetScalar(param[2]) >= 1.0f;
                            stateBlock = param[3].Value;

                            if (StateBlocks[stateBlock].Y == 0.0f || trigger)
                            {
                                // first time executing or needs to replace the held value
                                StateBlocks[stateBlock].X = value;
                                StateBlocks[stateBlock].Y = 1.0f;
                            }
                            else
                            {
                                // retrieve the held value
                                value = StateBlocks[stateBlock].X;
                            }

                            SetRegister(param[0], value);
                        }
                        break;
                    case Dat10Synth.Opcode.DECIMATE_BUFFER:
                        Decimate(GetBuffer(param[0]), GetScalar(param[1]), GetScalar(param[2]), ref StateBlocks[param[3].Value]);
                        break;
                    case Dat10Synth.Opcode.COUNTER:
                    case Dat10Synth.Opcode.COUNTER_TRIGGER:
                        SetRegister(param[0], Counter(
                            inst.Opcode == Dat10Synth.Opcode.COUNTER,
                            GetScalar(param[1]),
                            GetScalar(param[2]),
                            GetScalar(param[3]),
                            GetScalar(param[4]),
                            ref StateBlocks[param[5].Value]));
                        break;
                    case Dat10Synth.Opcode.GATE_BUFFER_BUFFER:
                        a = GetBuffer(param[0]);
                        b = GetBuffer(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = Gate(a[i], b[i]);
                        break;
                    case Dat10Synth.Opcode.GATE_BUFFER_SCALAR:
                        a = GetBuffer(param[0]);
                        scalar = GetScalar(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = Gate(a[i], scalar);
                        break;
                    case Dat10Synth.Opcode.GATE_SCALAR_SCALAR:
                        SetRegister(param[0], Gate(GetScalar(param[1]), GetScalar(param[2])));
                        break;
                    case Dat10Synth.Opcode.SMALL_DELAY_FRAC:
                        SmallDelay(GetBuffer(param[0]), GetScalar(param[1]), 0.0f, ref StateBlocks[param[inst.NumberOfInputs].Value], true);
                        break;
                    case Dat10Synth.Opcode.SMALL_DELAY_NON_INTERP:
                        SmallDelay(GetBuffer(param[0]), GetScalar(param[1]), 0.0f, ref StateBlocks[param[inst.NumberOfInputs].Value], false);
                        break;
                    case Dat10Synth.Opcode.SMALL_DELAY_FRAC_FEEDBACK:
                        SmallDelay(GetBuffer(param[0]), GetScalar(param[1]), GetScalar(param[2]), ref StateBlocks[param[3].Value], true);
                        break;
                    case Dat10Synth.Opcode.SMALL_DELAY_NON_INTERP_FEEDBACK:
                        SmallDelay(GetBuffer(param[0]), GetScalar(param[1]), GetScalar(param[2]), ref StateBlocks[param[3].Value], false);
                        break;
                    case Dat10Synth.Opcode.TRIGGER_DIFF:
                        SetRegister(param[0], TriggerDiff(GetScalar(param[1]), GetScalar(param[2]), ref StateBlocks[param[3].Value]));
                        break;
                    case Dat10Synth.Opcode.RANDOM_ON_INIT:
                        {
                            var min = GetScalar(param[1]);
                            var max = GetScalar(param[2]);
                            stateBlock = param[3].Value;

                            float randValue;
                            if (StateBlocks[stateBlock].Y == 0.0f)
                            {
                                // first time executing
                                randValue = rnd.NextFloat(min, max);
                                StateBlocks[stateBlock].X = randValue;
                                StateBlocks[stateBlock].Y = 1.0f;
                            }
                            else
                            {
                                // use previous value
                                randValue = StateBlocks[stateBlock].X;
                            }
                            SetRegister(param[0], randValue);
                        }
                        break;
                    case Dat10Synth.Opcode.FILL_BUFFER:
                        a = GetBuffer(param[0]);
                        scalar = GetScalar(param[1]);
                        for (int i = 0; i < BufferSize; i++)
                            a[i] = scalar;
                        break;
                    case Dat10Synth.Opcode.AWProcess:
                        AWFilter(GetBuffer(param[0]), GetBuffer(param[1]), GetBuffer(param[2]), ref StateBlocks[param[3].Value]);
                        break;
                    case Dat10Synth.Opcode.LERP_BUFFER_BUFFER:
                        {
                            var t = GetBuffer(param[0]);
                            var min = GetBuffer(param[1]);
                            var max = GetBuffer(param[2]);
                            for (int i = 0; i < BufferSize; i++)
                                t[i] = Lerp(t[i], min[i], max[i]);
                        }
                        break;
                    case Dat10Synth.Opcode.BiquadCoefficients_HighShelf_1:
                    case Dat10Synth.Opcode.BiquadCoefficients_HighShelf_2: // both opcodes do the same thing
                        BiquadCoefficientsHighShelf(
                            GetScalar(param[5]),
                            GetScalar(param[6]),
                            GetScalar(param[7]),
                            out Registers[param[0].Value & 0xFF],
                            out Registers[param[1].Value & 0xFF],
                            out Registers[param[2].Value & 0xFF],
                            out Registers[param[3].Value & 0xFF],
                            out Registers[param[4].Value & 0xFF]);
                        break;
                    case Dat10Synth.Opcode.BiquadCoefficients_LowShelf_1:
                    case Dat10Synth.Opcode.BiquadCoefficients_LowShelf_2: // both opcodes do the same thing
                        BiquadCoefficientsLowShelf(
                            GetScalar(param[5]),
                            GetScalar(param[6]),
                            GetScalar(param[7]),
                            out Registers[param[0].Value & 0xFF],
                            out Registers[param[1].Value & 0xFF],
                            out Registers[param[2].Value & 0xFF],
                            out Registers[param[3].Value & 0xFF],
                            out Registers[param[4].Value & 0xFF]);
                        break;
                    case Dat10Synth.Opcode.SWITCH_NORM_BUFFER:
                    case Dat10Synth.Opcode.SWITCH_INDEX_BUFFER:
                    case Dat10Synth.Opcode.SWITCH_LERP_BUFFER:
                    case Dat10Synth.Opcode.SWITCH_EQUAL_POWER_BUFFFER:
                        {
                            var outputBuffer = GetBuffer(param[0]);
                            var value = GetScalar(param[1]);
                            var buffers = new float[inst.NumberOfInputs - 1][];
                            buffers[0] = outputBuffer;
                            for (int i = 1; i < buffers.Length; i++)
                                buffers[i] = GetBuffer(param[2 + i - 1]);

                            switch (inst.Opcode)
                            {
                                case Dat10Synth.Opcode.SWITCH_NORM_SCALAR:
                                    SwitchNorm(value, buffers, outputBuffer);
                                    break;
                                case Dat10Synth.Opcode.SWITCH_INDEX_SCALAR:
                                    SwitchIndex(value, buffers, outputBuffer);
                                    break;
                                case Dat10Synth.Opcode.SWITCH_LERP_SCALAR:
                                    SwitchLerp(value, buffers, outputBuffer);
                                    break;
                                case Dat10Synth.Opcode.SWITCH_EQUAL_POWER_SCALAR:
                                    SwitchEqualPower(value, buffers, outputBuffer);
                                    break;
                            }
                        }
                        break;
                    case Dat10Synth.Opcode.SWITCH_NORM_SCALAR:
                    case Dat10Synth.Opcode.SWITCH_INDEX_SCALAR:
                    case Dat10Synth.Opcode.SWITCH_LERP_SCALAR:
                    case Dat10Synth.Opcode.SWITCH_EQUAL_POWER_SCALAR:
                        {
                            var value = GetScalar(param[1]);
                            var scalars = new float[inst.NumberOfInputs - 1];
                            for (int i = 0; i < scalars.Length; i++)
                                scalars[i] = GetScalar(param[2 + i]);

                            float result = 0.0f;
                            switch (inst.Opcode)
                            {
                                case Dat10Synth.Opcode.SWITCH_NORM_SCALAR:
                                    result = SwitchNorm(value, scalars);
                                    break;
                                case Dat10Synth.Opcode.SWITCH_INDEX_SCALAR:
                                    result = SwitchIndex(value, scalars);
                                    break;
                                case Dat10Synth.Opcode.SWITCH_LERP_SCALAR:
                                    result = SwitchLerp(value, scalars);
                                    break;
                                case Dat10Synth.Opcode.SWITCH_EQUAL_POWER_SCALAR:
                                    result = SwitchEqualPower(value, scalars);
                                    break;
                            }

                            SetRegister(param[0], result);
                        }
                        break;
                    case Dat10Synth.Opcode.AllpassProcess_BUFFER_SCALAR:
                        AllpassFilter(GetBuffer(param[0]), GetScalar(param[1]), ref StateBlocks[param[2].Value]);
                        break;
                    case Dat10Synth.Opcode.AllpassProcess_BUFFER_BUFFER:
                        AllpassFilter(GetBuffer(param[0]), GetBuffer(param[1]), ref StateBlocks[param[2].Value]);
                        break;
                    case Dat10Synth.Opcode.FINISH:
                        frameFinished = true;
                        break;
                    default:
                        throw new NotImplementedException(inst.Opcode.ToString() + " (line " + (insti + 1).ToString() + ")");
                }
            }

            FrameSynthesized?.Invoke(this, EventArgs.Empty);
        }

        private float[] GetBuffer(Dat10Synth.Parameter parameter)
        {
            return Buffers[parameter.Value & 0xFF];
        }

        private float GetRegister(Dat10Synth.Parameter parameter)
        {
            return Registers[parameter.Value & 0xFF];
        }

        private void SetRegister(Dat10Synth.Parameter parameter, float value)
        {
            Registers[parameter.Value & 0xFF] = value;
        }

        private float GetScalar(Dat10Synth.Parameter parameter)
        {
            var scalarId = parameter.Value;
            if ((scalarId & 0xFF00) == 0x100)
            {
                return Registers[scalarId & 0xFF];
            }

            switch (scalarId & 0xFF)
            {
                case 0: return 0.0f;
                case 1: return 1.0f;
                default: return Synth.Constants[(scalarId & 0xFF) - 2];
            }
        }

        private float ConvertToDenormalized(float v)
        {
            return v * 2.0f - 1.0f;
        }

        private float ConvertToNormalized(float v)
        {
            return (v + 1.0f) * 0.5f;
        }

        private float Sine(float v)
        {
            return (float)Math.Sin(v * 2.0f * Math.PI);
        }

        private float Cosine(float v)
        {
            return (float)Math.Cos(v * 2.0f * Math.PI);
        }

        private float Triangle(float v)
        {
            var tri = Math.Abs(1.0f - (v * 2.0f));
            tri = 1.0f - (tri * 2.0f);
            return tri;
        }

        private float Square(float v)
        {
            return v >= 0.5f ? 1.0f : -1.0f;
        }

        private float Saw(float v)
        {
            return -(v * 2.0f - 1.0f);
        }

        private float Rescale(float value, float min, float max, float newMin, float newMax)
        {
            float percent = (value - min) / (max - min);
            if (percent < 0.0f) percent = 0.0f;
            else if (percent > 1.0f) percent = 1.0f;

            return (newMax - newMin) * percent + newMin;
        }

        private float Lerp(float t, float min, float max)
        {
            return (max - min) * t + min;
        }

        private float HardKnee(float sample, float threshold)
        {
            float result;

            if (sample < 0.0f)
                result = 0.0f;
            else
                result = (sample / threshold) * 0.5f;

            if (sample >= threshold)
                result = (((sample - threshold) / (1.0f - threshold)) + 1.0f) * 0.5f;

            if (sample >= 1.0f)
                result = 1.0f;

            return result;
        }

        private float Modulo(float a, float b)
        {
            return ((a % b) + b) % b;
        }

        private float Pow(float a, float b)
        {
            return (float)Math.Pow(Math.Max(0, a), b);
        }

        // https://newt.phys.unsw.edu.au/jw/notes.html
        private float NoteToFrequency(float midiNote)
        {
            // A4 (note #69) = 440Hz
            return 440.0f * (float)Math.Pow(2.0f, (midiNote - 69) / 12.0f);
        }

        private void Decimate(float[] buffer, float scalar1, float deltaPerSample, ref StateBlock stateBlock)
        {
            // State block:
            //  X -> t: samples counter, each sample adds deltaPerSample, once it reaches 1.0 the lastSample value is updated
            //  Y -> lastSample from the previous execution

            float t = stateBlock.X;
            float lastSample = stateBlock.Y;

            // TODO(alexguirre): figure out the meaning of this formula and constants in Decimate
            float step = (float)Math.Pow(2.0f, scalar1 * 16.0f);
            for (int i = 0; i < BufferSize; i++)
                buffer[i] = (float)Math.Truncate(buffer[i] * step) / step;

            if (deltaPerSample < 1.0f)
            {
                for (int i = 0; i < BufferSize; i++)
                {
                    if (t >= 1.0f)
                    {
                        t -= 1.0f;
                        lastSample = buffer[i];
                    }

                    buffer[i] = lastSample;
                    t += deltaPerSample;
                }
            }

            stateBlock.X = t;
            stateBlock.Y = lastSample;
        }

        private float Counter(bool returnCounter, float setToZeroTrigger, float incrementTrigger, float decrementTrigger, float maxCounter, ref StateBlock stateBlock)
        {
            // State block:
            //  X -> counter value

            var setToZero = setToZeroTrigger >= 1.0f;
            var increment = incrementTrigger >= 1.0f;
            var decrement = decrementTrigger >= 1.0f;

            var trigger = 0.0f;
            var counter = stateBlock.X;
            var reachedMax = maxCounter > 0.0f && counter >= maxCounter;
            if (setToZero || reachedMax)
            {
                counter = 0.0f;
                trigger = 1.0f;
            }
            else
            {
                if (increment) counter += 1.0f;
                if (decrement) counter -= 1.0f;
            }
            stateBlock.X = counter;
            return returnCounter ? counter : trigger;
        }

        private float Gate(float value, float threshold)
        {
            return value < threshold ? 0.0f : 1.0f;
        }

        private float HardClip(float value, float threshold)
        {
            float valuePercent = Math.Abs(value) / Math.Max(0.0001f, threshold);
            valuePercent = Math.Max(0.0f, Math.Min(1.0f, valuePercent));
            return valuePercent * threshold * Math.Sign(value);
        }

        private float SoftClip(float value, float threshold)
        {
            float valuePercent = Math.Abs(value) / Math.Max(0.0001f, threshold);
            valuePercent = Math.Max(0.0f, Math.Min(1.0f, valuePercent));
            valuePercent = (1.5f * valuePercent) - (0.5f * valuePercent * valuePercent * valuePercent);
            return valuePercent * threshold * Math.Sign(value);
        }

        private void Compress(float[] buffer, float scalar1_threshold, float scalar2_ratio, float scalar3_time1, float scalar4_time2, ref StateBlock stateBlock)
        {
            // State block:
            //  X -> ...

            // TODO: research COMPRESS_BUFFER
            float stateX = stateBlock.X;

            scalar1_threshold = Math.Max(0.000001f, scalar1_threshold);
            scalar2_ratio = Math.Max(0.000001f, scalar2_ratio);
            scalar3_time1 = Math.Max(0.000001f, scalar3_time1);
            scalar4_time2 = Math.Max(0.000001f, scalar4_time2);

            float v1_slopeFactor = Math.Max(0.000001f, 1.0f - (1.0f / scalar2_ratio));
            float v2_threshold = (float)Math.Pow(scalar1_threshold, v1_slopeFactor);
            float v3 = (float)Math.Exp(-2.2f / (scalar3_time1 * SampleRate));
            float v4 = (float)Math.Exp(-2.2f / (scalar4_time2 * SampleRate));

            for (int i = 0; i < BufferSize; i++)
            {
                float sample = Math.Max(0.000001f, Math.Abs(buffer[i]));
                float v5 = Math.Max(1.0f, (float)Math.Pow(sample, v1_slopeFactor) / v2_threshold);
                float v6 = v5 - stateX;
                float v7 = v6 < 0.0f ? v4 : v3;
                stateX = (v6 * (1.0f - v7)) + stateX;
                buffer[i] = 1.0f / stateX;
            }

            stateBlock.X = stateX;
        }

        // TODO(alexguirre): verify results of BiquadCoefficients
        // TODO(alexguirre): find better names for BiquadCoefficients local variables
        // https://webaudio.github.io/Audio-EQ-Cookbook/audio-eq-cookbook.html
        private void BiquadCoefficientsLowPass(bool unkFlag, float frequency, float unk, out float b0, out float b1, out float b2, out float a1, out float a2)
        {
            frequency = Math.Max(0.01f, Math.Min(20000.16f, frequency));

            float w = (float)(2.0 * Math.PI * frequency / SampleRate);
            float v11 = unkFlag ? 0.5f : 1.0f;
            float cosW = (float)Math.Cos(w);
            float alpha = (float)Math.Sin(w) * (float)Math.Sinh(0.5f / (v11 * unk));

            b0 = (1.0f - cosW) / 2.0f;
            b1 = 1.0f - cosW;
            b2 = (1.0f - cosW) / 2.0f;

            float a0;
            a0 = 1.0f + alpha;
            a1 = -2.0f * cosW;
            a2 = 1.0f - alpha;

            // normalize for a0 to be 1
            b0 /= a0; b1 /= a0; b2 /= a0;
            a1 /= a0; a2 /= a0;
        }

        private void BiquadCoefficientsHighPass(bool unkFlag, float frequency, float unk, out float b0, out float b1, out float b2, out float a1, out float a2)
        {
            frequency = Math.Max(0.01f, Math.Min(20000.16f, frequency));

            float w = (float)(2.0 * Math.PI * frequency / SampleRate);
            float v11 = unkFlag ? 0.5f : 1.0f;
            float cosW = (float)Math.Cos(w);
            float alpha = (float)Math.Sin(w) * (float)Math.Sinh(0.5f / (v11 * unk));

            b0 = (1.0f + cosW) / 2.0f;
            b1 = -(1.0f + cosW);
            b2 = (1.0f + cosW) / 2.0f;

            float a0;
            a0 = 1.0f + alpha;
            a1 = -2.0f * cosW;
            a2 = 1.0f - alpha;

            // normalize for a0 to be 1
            b0 /= a0; b1 /= a0; b2 /= a0;
            a1 /= a0; a2 /= a0;
        }

        private void BiquadCoefficientsBandPass(float centerFrequency, float freq2, out float b0, out float b1, out float b2, out float a1, out float a2)
        {
            centerFrequency = Math.Max(0.01f, Math.Min(20000.16f, centerFrequency));
            freq2 = Math.Max(0.000001f, Math.Min(centerFrequency * 2.0f, freq2));

            float w = (float)(2.0 * Math.PI * centerFrequency / SampleRate);
            float cosW = (float)Math.Cos(w);
            float v14 = 1.0f / (float)Math.Tan(Math.PI * freq2 / SampleRate);

            b0 = 1.0f;
            b1 = 0.0f;
            b2 = -1.0f;

            float a0;
            a0 = 1.0f + v14;
            a1 = -(2.0f * cosW) * v14;
            a2 = v14 - 1.0f;

            // normalize for a0 to be 1
            b0 /= a0; b1 /= a0; b2 /= a0;
            a1 /= a0; a2 /= a0;
        }

        private void BiquadCoefficientsBandStop(float centerFrequency, float freq2, out float b0, out float b1, out float b2, out float a1, out float a2)
        {
            centerFrequency = Math.Max(0.01f, Math.Min(24000.0f, centerFrequency));
            freq2 = Math.Max(0.000001f, Math.Min(centerFrequency * 2.0f, freq2));

            float w = (float)(2.0f * Math.PI * centerFrequency / SampleRate);
            float cosW = (float)Math.Cos(w);
            float v15 = (float)Math.Tan(Math.PI * freq2 / SampleRate);

            b0 = 1.0f;
            b1 = -2.0f * cosW;
            b2 = b0;

            float a0;
            a0 = 1.0f + v15;
            a1 = b1;
            a2 = 1.0f - v15;

            // normalize for a0 to be 1
            b0 /= a0; b1 /= a0; b2 /= a0;
            a1 /= a0; a2 /= a0;
        }

        private void BiquadCoefficientsPeakingEQ(float centerFrequency, float freq2, float gain, out float b0, out float b1, out float b2, out float a1, out float a2)
        {
            centerFrequency = Math.Max(0.000001f, Math.Min(20000.0f, centerFrequency));
            freq2 = Math.Max(0.000001f, Math.Min(centerFrequency * 2.0f, freq2));

            float w = (float)(2.0f * Math.PI * centerFrequency / SampleRate);
            float q = centerFrequency / freq2;
            float alpha = (float)Math.Sin(w) / (2.0f * q);
            float cosW = (float)Math.Cos(w);
            float A = Math.Max(0.000001f, gain * (float)Math.Sqrt(2)); // TODO(alexguirre): this should be amp = 10.0 ** (gain_db/40.0), where does sqrt(2) come from? maybe gain not in dB?

            b0 = 1.0f + alpha * A;
            b1 = -2.0f * cosW;
            b2 = 1.0f - alpha * A;

            float a0;
            a0 = 1.0f + alpha / A;
            a1 = -2.0f * cosW;
            a2 = 1.0f - alpha / A;

            // normalize for a0 to be 1
            b0 /= a0; b1 /= a0; b2 /= a0;
            a1 /= a0; a2 /= a0;
        }

        private void BiquadCoefficientsHighShelf(float frequency, float freq2, float A, out float b0, out float b1, out float b2, out float a1, out float a2)
        {
            frequency = Math.Max(0.01f, Math.Min(20000.16f, frequency));
            freq2 = Math.Max(0.000001f, Math.Min(frequency * 2.0f, freq2));
            A = Math.Max(0.000001f, A);

            float w = (float)(2.0f * Math.PI * frequency / SampleRate);
            float q = frequency / freq2;
            float alpha = (float)Math.Sin(w) / (2.0f * q);
            float cosW = (float)Math.Cos(w);
            float sqrtA = 2.0f * (float)Math.Sqrt(A) * alpha;

            b0 = (A + 1.0f) + (A - 1.0f) * cosW + sqrtA;
            b1 = -2.0f * ((A - 1.0f) + (A + 1.0f) * cosW);
            b2 = (A + 1.0f) + (A - 1.0f) * cosW - sqrtA;

            float a0;
            a0 = A * ((A + 1.0f) - (A - 1.0f) * cosW + sqrtA);
            a1 = 2.0f * A * ((A - 1.0f) - (A + 1.0f) * cosW);
            a2 = A * ((A + 1.0f) - (A - 1.0f) * cosW - sqrtA);

            // normalize for a0 to be 1
            b0 /= a0; b1 /= a0; b2 /= a0;
            a1 /= a0; a2 /= a0;
        }

        private void BiquadCoefficientsLowShelf(float frequency, float freq2, float A, out float b0, out float b1, out float b2, out float a1, out float a2)
        {
            frequency = Math.Max(0.01f, Math.Min(20000.16f, frequency));
            freq2 = Math.Max(0.000001f, Math.Min(frequency * 2.0f, freq2));
            A = Math.Max(0.000001f, A);

            float w = (float)(2.0f * Math.PI * frequency / SampleRate);
            float q = frequency / freq2;
            float alpha = (float)Math.Sin(w) / (2.0f * q);
            float cosW = (float)Math.Cos(w);
            float sqrtA = 2.0f * (float)Math.Sqrt(A) * alpha;

            b0 = (A + 1.0f) - (A - 1.0f) * cosW + sqrtA;
            b1 = 2.0f * ((A - 1.0f) - (A + 1.0f) * cosW);
            b2 = (A + 1.0f) - (A - 1.0f) * cosW - sqrtA;

            float a0;
            a0 = A * ((A + 1.0f) + (A - 1.0f) * cosW + sqrtA);
            a1 = -2.0f * A * ((A - 1.0f) + (A + 1.0f) * cosW);
            a2 = A * ((A + 1.0f) + (A - 1.0f) * cosW - sqrtA);

            // normalize for a0 to be 1
            b0 /= a0; b1 /= a0; b2 /= a0;
            a1 /= a0; a2 /= a0;
        }

        private void BiquadFilter2Pole(float[] buffer, float b0, float b1, float b2, float a1, float a2, ref StateBlock stateBlock)
        {
            float pole1 = stateBlock.X;
            float pole2 = stateBlock.Y;

            for (int i = 0; i < BufferSize; i++)
            {
                float s = buffer[i];
                float x = (s * b0) + pole1;
                pole1 = (s * b1) - (x * a1) + pole2;
                pole2 = (s * b2) - (x * a2);
                buffer[i] = x;
            }

            stateBlock.X = pole1;
            stateBlock.Y = pole2;
        }

        private void BiquadFilter4Pole(float[] buffer, float b0, float b1, float b2, float a1, float a2, ref StateBlock stateBlock)
        {
            float pole1 = stateBlock.X;
            float pole2 = stateBlock.Y;
            float pole3 = stateBlock.Z;
            float pole4 = stateBlock.W;

            for (int i = 0; i < BufferSize; i++)
            {
                float s = buffer[i];
                float x = (s * b0) + pole1;
                pole1 = (s * b1) - (x * a1) + pole2;
                pole2 = (s * b2) - (x * a2);
                float y = (x * b0) + pole3;
                pole3 = (x * b1) - (y * a1) + pole4;
                pole4 = (x * b2) - (y * a2);
                buffer[i] = y;
            }

            stateBlock.X = pole1;
            stateBlock.Y = pole2;
            stateBlock.Z = pole3;
            stateBlock.W = pole4;
        }

        // https://www.earlevel.com/main/2012/12/15/a-one-pole-filter/
        // TODO: verify OnePoleLPF/HPF results
        private void OnePoleLPF(float[] buffer, float[] frequencies, ref StateBlock stateBlock)
        {
            for (int i = 0; i < BufferSize; i++)
            {
                buffer[i] = OnePoleLPF(buffer[i], frequencies[i], ref stateBlock);
            }
        }

        private void OnePoleLPF(float[] buffer, float frequency, ref StateBlock stateBlock)
        {
            for (int i = 0; i < BufferSize; i++)
            {
                buffer[i] = OnePoleLPF(buffer[i], frequency, ref stateBlock);
            }
        }

        private float OnePoleLPF(float sample, float frequency, ref StateBlock stateBlock)
        {
            float previousSample = stateBlock.X;

            float b1 = (float)Math.Exp(-2.0f * Math.PI * frequency * 256.0f / SampleRate);
            float a0 = 1.0f - b1;

            float s = a0 * sample + (b1 * previousSample);
            stateBlock.X = s;
            return s;
        }

        private void OnePoleHPF(float[] buffer, float[] frequencies, ref StateBlock stateBlock)
        {
            for (int i = 0; i < BufferSize; i++)
            {
                buffer[i] = OnePoleHPF(buffer[i], frequencies[i], ref stateBlock);
            }
        }

        private void OnePoleHPF(float[] buffer, float frequency, ref StateBlock stateBlock)
        {
            for (int i = 0; i < BufferSize; i++)
            {
                buffer[i] = OnePoleHPF(buffer[i], frequency, ref stateBlock);
            }
        }

        private float OnePoleHPF(float sample, float frequency, ref StateBlock stateBlock)
        {
            return sample - OnePoleLPF(sample, frequency, ref stateBlock);
        }

        private void EnvelopeFollower(float[] buffer, float a2, float a3, ref StateBlock stateBlock)
        {
            // State block:
            //  X -> previous follower value

            // TODO(alexguirre): verify this is the same formula as in-game
            // TODO(alexguirre): better names for both EnvelopeFollower methods
            a2 = (float)Math.Max(0.00001, a2);
            a3 = (float)Math.Max(0.00001, a3);

            float prevFollower = stateBlock.X;
            for (int i = 0; i < BufferSize; i++)
            {
                float scalar = Math.Abs(buffer[i]);
                float v12;
                if (scalar < prevFollower)
                    v12 = 1.0f / (a3 * 187.5f);
                else
                    v12 = 1.0f / (a2 * 187.5f);

                float v13 = (prevFollower - scalar) * (float)Math.Pow(0.01f, v12) + scalar;

                float follower = Math.Max(0.0f, v13);
                buffer[i] = follower;
                prevFollower = follower;
            }
            stateBlock.X = prevFollower;
        }

        private float EnvelopeFollower(float scalar, float a2, float a3, ref StateBlock stateBlock)
        {
            // State block:
            //  X -> previous follower value

            scalar = Math.Abs(scalar);
            a2 = (float)Math.Max(0.00001, a2);
            a3 = (float)Math.Max(0.00001, a3);

            float prevFollower = stateBlock.X;
            float v12;
            if (scalar < prevFollower)
                v12 = 1.0f / (a3 * 187.5f);
            else
                v12 = 1.0f / (a2 * 187.5f);

            float v13 = (prevFollower - scalar) * (float)Math.Pow(0.01f, v12) + scalar;

            float follower = Math.Max(0.0f, v13);
            stateBlock.X = follower;
            return follower;
        }

        private float TriggerLatch(float value, float triggerLimit, ref StateBlock stateBlock)
        {
            return TriggerLatch(value >= (triggerLimit - 0.000001f), ref stateBlock);
        }

        private float TriggerLatch(float[] buffer, float triggerLimit, ref StateBlock stateBlock)
        {
            bool triggered = false;
            for (int i = 0; !triggered && i < BufferSize; i++)
            {
                triggered = buffer[i] >= (triggerLimit - 0.000001f);
            }
            return TriggerLatch(triggered, ref stateBlock);
        }

        private float TriggerLatch(float[] buffer, float[] triggerLimitBuffer, ref StateBlock stateBlock)
        {
            bool triggered = false;
            for (int i = 0; !triggered && i < BufferSize; i++)
            {
                triggered = buffer[i] >= (triggerLimitBuffer[i] - 0.000001f);
            }
            return TriggerLatch(triggered, ref stateBlock);
        }

        private float TriggerLatch(bool triggered, ref StateBlock stateBlock)
        {
            // State block:
            //  X -> latch SET

            bool set = stateBlock.X != 0.0f;
            float latch = 0.0f;
            if (triggered && !set)
                latch = 1.0f;

            if (triggered != set)
            {
                stateBlock.X = triggered ? 1.0f : 0.0f;
            }
            return latch;
        }

        private float TriggerDiff(float value, float triggerLimit, ref StateBlock stateBlock)
        {
            // State block:
            //  X -> previous value

            float diff = Math.Abs(stateBlock.X - value);
            stateBlock.X = value;
            bool triggered = diff >= (triggerLimit - 0.000001f);
            return triggered ? 1.0f : 0.0f;
        }

        private void OscillatorRamp(float[] buffer, ref StateBlock stateBlock)
        {
            // TODO(alexguirre): better names in OSC_RAMP_BUFFER_BUFFER
            var stateX = stateBlock.X;
            for (int i = 0; i < BufferSize; i++)
            {
                var frequency = Math.Max(0.0f, Math.Min(24000.0f, buffer[i])) / SampleRate;
                var v3 = frequency + stateX;
                var v4 = v3 - (float)Math.Truncate(v3);
                buffer[i] = v4;
                stateX = v4 + frequency;
            }
            stateBlock.X = stateX;
        }

        private void OscillatorRamp(float[] buffer, float frequency, ref StateBlock stateBlock)
        {
            // TODO(alexguirre): better names in OSC_RAMP_BUFFER_SCALAR
            var v1 = frequency / SampleRate;
            var v2 = v1 * 4.0f; // TODO(alexguirre): IS THIS 4 because of 4 rows in a SIMD vector? should it be just 1 here?
            var v3 = v1 * 3.0f + stateBlock.X;
            for (int i = 0; i < BufferSize; i++)
            {
                var v4 = v3 - (float)Math.Truncate(v3);
                buffer[i] = v4;
                v3 = v4 + v2;
            }
            stateBlock.X = v3;
        }

        private float OscillatorRamp(float frequency, ref StateBlock stateBlock)
        {
            // TODO(alexguirre): better names in OSC_RAMP_SCALAR
            var v2 = Math.Max(1.0f, frequency / 187.5f);
            var v4 = v2 + stateBlock.X;
            if (v4 >= 1.0f)
                v4 -= 1.0f;
            var v3 = v4;
            var result = stateBlock.X;
            stateBlock.X = v3;
            return result;
        }

        private enum EnvelopeGenState { Idle = 0, Predelay, Attack, Decay, HoldUntilTrigger, Hold, Release, Finish }
        private enum EnvelopeReleaseType { Linear, Exponential }
        private enum EnvelopeTriggerMode { OneShot, Retrigger, Interruptible }

        private float EnvelopeGen(Dat10Synth.Opcode envelopeGenOpcode, float[] buffer, ref StateBlock stateBlock, float predelay, float attack, float decay, float sustain, float hold, float release, float trigger)
        {
            EnvelopeReleaseType releaseType;
            switch (envelopeGenOpcode)
            {
                case Dat10Synth.Opcode.ENVELOPE_GEN__R_LINEAR_T_INTERRUPTIBLE:
                case Dat10Synth.Opcode.ENVELOPE_GEN__R_LINEAR_T_ONE_SHOT:
                case Dat10Synth.Opcode.ENVELOPE_GEN__R_LINEAR_T_RETRIGGER:
                    releaseType = EnvelopeReleaseType.Linear;
                    break;
                case Dat10Synth.Opcode.ENVELOPE_GEN__R_EXP_T_INTERRUPTIBLE:
                case Dat10Synth.Opcode.ENVELOPE_GEN__R_EXP_T_ONE_SHOT:
                case Dat10Synth.Opcode.ENVELOPE_GEN__R_EXP_T_RETRIGGER:
                    releaseType = EnvelopeReleaseType.Exponential;
                    break;

                default: throw new ArgumentOutOfRangeException("Not an ENVELOPE_GEN_* opcode");
            }

            EnvelopeTriggerMode triggerMode;
            switch (envelopeGenOpcode)
            {
                case Dat10Synth.Opcode.ENVELOPE_GEN__R_LINEAR_T_ONE_SHOT:
                case Dat10Synth.Opcode.ENVELOPE_GEN__R_EXP_T_ONE_SHOT:
                    triggerMode = EnvelopeTriggerMode.OneShot;
                    break;
                case Dat10Synth.Opcode.ENVELOPE_GEN__R_LINEAR_T_RETRIGGER:
                case Dat10Synth.Opcode.ENVELOPE_GEN__R_EXP_T_RETRIGGER:
                    triggerMode = EnvelopeTriggerMode.Retrigger;
                    break;
                case Dat10Synth.Opcode.ENVELOPE_GEN__R_LINEAR_T_INTERRUPTIBLE:
                case Dat10Synth.Opcode.ENVELOPE_GEN__R_EXP_T_INTERRUPTIBLE:
                    triggerMode = EnvelopeTriggerMode.Interruptible;
                    break;

                default: throw new ArgumentOutOfRangeException("Not an ENVELOPE_GEN_* opcode");
            }

            return EnvelopeGen(releaseType, triggerMode, buffer, ref stateBlock, predelay, attack, decay, sustain, hold, release, trigger);
        }

        private float EnvelopeGen(EnvelopeReleaseType releaseType, EnvelopeTriggerMode triggerMode, float[] buffer, ref StateBlock stateBlock, float predelay, float attack, float decay, float sustain, float hold, float release, float trigger)
        {
            // State block:
            //  X -> envelope state : 8 bits | envelope state remaining samples : 24 bits
            //  Y -> release start Y: level at which the release ramp starts
            //  Z -> ramp Y: current height of a ramp
            //  W -> ramp slope: difference between samples of a ramp

            uint stateInfo = BitConverter.ToUInt32(BitConverter.GetBytes(stateBlock.X), 0);
            float stateReleaseStartY = stateBlock.Y;
            float stateRampY = stateBlock.Z;
            float stateRampSlope = stateBlock.W;
            EnvelopeGenState state = (EnvelopeGenState)(stateInfo & 0xFF);
            uint stateRemainingSamples = stateInfo >> 8;
            bool stateFinished = stateRemainingSamples < BufferSize;
            bool clearBuffer = true;
            float finishedTrigger = 0.0f;
            bool triggered = trigger >= 1.0f;

            if (state != EnvelopeGenState.Idle && triggerMode == EnvelopeTriggerMode.Interruptible && triggered)
                state = EnvelopeGenState.Idle; // interrupted, reset the envelope

            switch (state)
            {
                case EnvelopeGenState.Idle:
                    if (triggered)
                    {
                        state = EnvelopeGenState.Predelay;
                        stateRemainingSamples = (uint)(Math.Max(predelay, 0.0f) * SampleRate);
                        stateFinished = stateRemainingSamples < BufferSize;
                        goto case EnvelopeGenState.Predelay;
                    }
                    break;

                case EnvelopeGenState.Predelay:
                    if (stateFinished)
                    {
                        state = EnvelopeGenState.Attack;
                        stateRemainingSamples = EnvelopeComputeRamp(attack, out stateRampSlope);
                        stateRampY = 0.0f;
                        stateFinished = stateRemainingSamples < BufferSize;
                        goto case EnvelopeGenState.Attack;
                    }
                    else
                    {
                        // nothing
                    }
                    break;

                case EnvelopeGenState.Attack:
                    if (stateFinished)
                    {
                        state = EnvelopeGenState.Decay;
                        stateRemainingSamples = EnvelopeComputeRamp(decay, out stateRampSlope);
                        stateRampY = 0.0f;
                        stateFinished = stateRemainingSamples < BufferSize;
                        goto case EnvelopeGenState.Decay;
                    }
                    else
                    {
                        EnvelopeProcessLinearRamp(buffer, ref stateRampY, stateRampSlope);
                        clearBuffer = false;
                    }
                    break;

                case EnvelopeGenState.Decay:
                    if (stateFinished)
                    {
                        state = EnvelopeGenState.HoldUntilTrigger;
                        goto case EnvelopeGenState.HoldUntilTrigger;
                    }
                    else
                    {
                        EnvelopeProcessLinearRamp(buffer, ref stateRampY, stateRampSlope);
                        EnvelopeProcessLinearRelease(buffer, 1.0f, sustain);
                        clearBuffer = false;
                    }
                    break;

                case EnvelopeGenState.HoldUntilTrigger: // trigger refers to 'hold >= 0.0f'
                    if (hold >= 0.0f)
                    {
                        state = EnvelopeGenState.Hold;
                        stateRemainingSamples = EnvelopeComputeRamp(hold, out stateRampSlope);
                        stateRampY = 0.0f;
                        stateFinished = stateRemainingSamples < BufferSize;
                        goto case EnvelopeGenState.Hold;
                    }
                    else
                    {
                        for (int i = 0; i < BufferSize; i++)
                        {
                            buffer[i] = sustain;
                        }
                        clearBuffer = false;
                    }
                    break;

                case EnvelopeGenState.Hold:
                    if (stateFinished)
                    {
                        state = EnvelopeGenState.Release;
                        stateRemainingSamples = EnvelopeComputeRamp(release, out stateRampSlope);
                        stateRampY = 0.0f;
                        stateReleaseStartY = sustain;
                        stateFinished = stateRemainingSamples < BufferSize;
                        goto case EnvelopeGenState.Release;
                    }
                    else
                    {
                        for (int i = 0; i < BufferSize; i++)
                        {
                            buffer[i] = sustain;
                        }
                        clearBuffer = false;
                    }
                    break;

                case EnvelopeGenState.Release:
                    if (stateFinished)
                    {
                        state = EnvelopeGenState.Finish;
                        goto case EnvelopeGenState.Finish;
                    }
                    else
                    {
                        EnvelopeProcessLinearRamp(buffer, ref stateRampY, stateRampSlope);

                        if (releaseType == EnvelopeReleaseType.Exponential)
                            EnvelopeProcessExponentialRelease(buffer, stateReleaseStartY);
                        else
                            EnvelopeProcessLinearRelease(buffer, stateReleaseStartY, 0.0f);
                        clearBuffer = false;
                    }
                    break;

                case EnvelopeGenState.Finish:
                    if (triggerMode != EnvelopeTriggerMode.OneShot || !triggered)
                        state = EnvelopeGenState.Idle; // restart the envelope
                    finishedTrigger = 1.0f;
                    break;
            }

            if (clearBuffer)
                for (int i = 0; i < BufferSize; i++)
                    buffer[i] = 0.0f;

            stateRemainingSamples -= BufferSize;
            stateBlock.X = BitConverter.ToSingle(BitConverter.GetBytes((byte)state  | (stateRemainingSamples << 8)), 0);
            stateBlock.Y = stateReleaseStartY;
            stateBlock.Z = stateRampY;
            stateBlock.W = stateRampSlope;
            return finishedTrigger;
        }

        /// <summary>
        /// Calculates the number of samples and slope needed to create a ramp from 0 to 1 in the given time.
        /// </summary>
        private uint EnvelopeComputeRamp(float seconds, out float rampSlope)
        {
            float maxSamples = Math.Max(0.0f, seconds * SampleRate);
            // calculate the multiple of BufferSize closest to maxSamples without exceeding it
            float samples = maxSamples - (maxSamples - ((float)Math.Truncate(maxSamples / BufferSize) * BufferSize));
            rampSlope = 1.0f / samples;
            return (uint)samples;
        }

        /// <summary>
        /// Fills the buffer with a linear ramp with the given slope starting at <paramref name="rampY"/>.
        /// </summary>
        private void EnvelopeProcessLinearRamp(float[] buffer, ref float rampY, float rampSlope)
        {
            for (int i = 0; i < BufferSize; i++)
            {
                buffer[i] = rampY;
                rampY += rampSlope;
            }
        }

        /// <summary>
        /// Converts a ramp with increasing slope from 0 to 1 to a ramp with decreasing slope from <paramref name="topY"/>
        /// to <paramref name="bottomY"/>.
        /// </summary>
        private void EnvelopeProcessLinearRelease(float[] buffer, float topY, float bottomY)
        {
            float diff = bottomY - topY;
            for (int i = 0; i < BufferSize; i++)
            {
                buffer[i] = (buffer[i] * diff) + topY;
            }
        }

        private void EnvelopeProcessExponentialRelease(float[] buffer, float topY)
        {
            // TODO(alexguirre): implement EnvelopeProcessExponentialRelease
            EnvelopeProcessLinearRelease(buffer, topY, 0.0f);
        }

        private struct TimedTriggerResult
        {
            public float Finished;
            public float AttackActive;
            public float DecayActive;
            public float HoldActive;
            public float ReleaseActive;
        }

        private TimedTriggerResult TimedTrigger(Dat10Synth.Opcode envelopeGenOpcode, ref StateBlock stateBlock, float trigger, float predelay, float attack, float decay, float hold, float release)
        {
            EnvelopeTriggerMode triggerMode;
            switch (envelopeGenOpcode)
            {
                case Dat10Synth.Opcode.TIMED_TRIGGER__T_ONE_SHOT:
                    triggerMode = EnvelopeTriggerMode.OneShot;
                    break;
                case Dat10Synth.Opcode.TIMED_TRIGGER__T_RETRIGGER:
                    triggerMode = EnvelopeTriggerMode.Retrigger;
                    break;
                case Dat10Synth.Opcode.TIMED_TRIGGER__T_INTERRUPTIBLE:
                    triggerMode = EnvelopeTriggerMode.Interruptible;
                    break;

                default: throw new ArgumentOutOfRangeException("Not an TIMED_TRIGGER_* opcode");
            }

            return TimedTrigger(triggerMode, ref stateBlock, trigger, predelay, attack, decay, hold, release);
        }

        // TODO(alexguirre): TimedTrigger may not be equivalent, game code has like 10 states, here I'm using the same states as the envelope gen
        // TODO(alexguirre): verify how TimedTrigger works in-game
        private TimedTriggerResult TimedTrigger(EnvelopeTriggerMode triggerMode, ref StateBlock stateBlock, float trigger, float predelay, float attack, float decay, float hold, float release)
        {
            // State block:
            //  X -> envelope state : 8 bits | envelope state remaining samples : 24 bits

            TimedTriggerResult result = default;
            uint stateInfo = BitConverter.ToUInt32(BitConverter.GetBytes(stateBlock.X), 0);
            EnvelopeGenState state = (EnvelopeGenState)(stateInfo & 0xFF);
            uint stateRemainingSamples = stateInfo >> 8;
            bool stateFinished = stateRemainingSamples < BufferSize;
            bool triggered = trigger >= 1.0f;

            if (state != EnvelopeGenState.Idle && triggerMode == EnvelopeTriggerMode.Interruptible && triggered)
                state = EnvelopeGenState.Idle; // interrupted, reset the envelope

            switch (state)
            {
                case EnvelopeGenState.Idle:
                    if (triggered)
                    {
                        state = EnvelopeGenState.Predelay;
                        stateRemainingSamples = (uint)(Math.Max(predelay, 0.0f) * SampleRate);
                        stateFinished = stateRemainingSamples < BufferSize;
                        goto case EnvelopeGenState.Predelay;
                    }
                    break;

                case EnvelopeGenState.Predelay:
                    if (stateFinished)
                    {
                        state = EnvelopeGenState.Attack;
                        stateRemainingSamples = (uint)(Math.Max(attack, 0.0f) * SampleRate);
                        stateFinished = stateRemainingSamples < BufferSize;
                        goto case EnvelopeGenState.Attack;
                    }
                    else
                    {
                        // nothing
                    }
                    break;

                case EnvelopeGenState.Attack:
                    if (stateFinished)
                    {
                        state = EnvelopeGenState.Decay;
                        stateRemainingSamples = (uint)(Math.Max(decay, 0.0f) * SampleRate);
                        stateFinished = stateRemainingSamples < BufferSize;
                        goto case EnvelopeGenState.Decay;
                    }
                    else
                    {
                        result.AttackActive = 1.0f;
                    }
                    break;

                case EnvelopeGenState.Decay:
                    if (stateFinished)
                    {
                        state = EnvelopeGenState.HoldUntilTrigger;
                        goto case EnvelopeGenState.HoldUntilTrigger;
                    }
                    else
                    {
                        result.DecayActive = 1.0f;
                    }
                    break;

                case EnvelopeGenState.HoldUntilTrigger: // trigger refers to 'hold >= 0.0f'
                    if (hold >= 0.0f)
                    {
                        state = EnvelopeGenState.Hold;
                        stateRemainingSamples = (uint)(Math.Max(hold, 0.0f) * SampleRate);
                        stateFinished = stateRemainingSamples < BufferSize;
                        goto case EnvelopeGenState.Hold;
                    }
                    else
                    {
                        result.HoldActive = 1.0f;
                    }
                    break;

                case EnvelopeGenState.Hold:
                    if (stateFinished)
                    {
                        state = EnvelopeGenState.Release;
                        stateRemainingSamples = (uint)(Math.Max(release, 0.0f) * SampleRate);
                        stateFinished = stateRemainingSamples < BufferSize;
                        goto case EnvelopeGenState.Release;
                    }
                    else
                    {
                        result.HoldActive = 1.0f;
                    }
                    break;

                case EnvelopeGenState.Release:
                    if (stateFinished)
                    {
                        state = EnvelopeGenState.Finish;
                        goto case EnvelopeGenState.Finish;
                    }
                    else
                    {
                        result.ReleaseActive = 1.0f;
                    }
                    break;

                case EnvelopeGenState.Finish:
                    if (triggerMode != EnvelopeTriggerMode.OneShot || !triggered)
                        state = EnvelopeGenState.Idle; // restart the envelope
                    result.Finished = 1.0f;
                    break;
            }

            stateRemainingSamples -= BufferSize;
            stateBlock.X = BitConverter.ToSingle(BitConverter.GetBytes((byte)state | (stateRemainingSamples << 8)), 0);
            return result;
        }

        private float SwitchNorm(float value, float[] scalars)
        {
            int index = (int)(scalars.Length * Math.Max(0.0f, Math.Min(1.0f, value)));
            if (index >= scalars.Length)
                index = scalars.Length - 1;

            return scalars[index];
        }

        private float SwitchIndex(float value, float[] scalars)
        {
            return scalars[(int)value % scalars.Length];
        }

        private float SwitchLerp(float value, float[] scalars)
        {
            int index = (int)value % scalars.Length;
            float t = value - (float)Math.Floor(value);

            if (index >= scalars.Length - 1 || t == 0.0f)
                return scalars[index];

            return Lerp(t, scalars[index], scalars[index + 1]);
        }

        private float SwitchEqualPower(float value, float[] scalars)
        {
            int index = (int)value % scalars.Length;
            float t = value - (float)Math.Floor(value);

            if (index >= scalars.Length - 1 || t == 0.0f)
                return scalars[index];

            float r = (float)(t * Math.PI * 0.5f);
            return (float)Math.Cos(r) * scalars[index] + (float)Math.Sin(r) * scalars[index + 1];
        }

        private void SwitchNorm(float value, float[][] buffers, float[] outputBuffer)
        {
            int index = (int)(buffers.Length * Math.Max(0.0f, Math.Min(1.0f, value)));
            if (index >= buffers.Length)
                index = buffers.Length - 1;

            if (buffers[index] != outputBuffer)
                Array.Copy(buffers[index], outputBuffer, BufferSize);
        }

        private void SwitchIndex(float value, float[][] buffers, float[] outputBuffer)
        {
            int index = (int)value % buffers.Length;

            if (buffers[index] != outputBuffer)
                Array.Copy(buffers[index], outputBuffer, BufferSize);
        }

        private void SwitchLerp(float value, float[][] buffers, float[] outputBuffer)
        {
            int index = (int)value % buffers.Length;
            float t = value - (float)Math.Floor(value);

            if (index >= buffers.Length - 1 || t == 0.0f)
            {
                if (buffers[index] != outputBuffer)
                    Array.Copy(buffers[index], outputBuffer, BufferSize);
                return;
            }

            var min = buffers[index];
            var max = buffers[index + 1];
            for (int i = 0; i < BufferSize; i++)
            {
                outputBuffer[i] = Lerp(t, min[i], max[i]);
            }
        }

        private void SwitchEqualPower(float value, float[][] buffers, float[] outputBuffer)
        {
            int index = (int)value % buffers.Length;
            float t = value - (float)Math.Floor(value);

            if (index >= buffers.Length - 1 || t == 0.0f)
            {
                if (buffers[index] != outputBuffer)
                    Array.Copy(buffers[index], outputBuffer, BufferSize);
                return;
            }

            float r = (float)(t * Math.PI * 0.5f);
            float cos = (float)Math.Cos(r);
            float sin = (float)Math.Sin(r);

            var a = buffers[index];
            var b = buffers[index + 1];
            for (int i = 0; i < BufferSize; i++)
            {
                outputBuffer[i] = cos * a[i] + sin * b[i];
            }
        }

        private void SmallDelay(float[] buffer, float delaySamples, float feedback, ref StateBlock stateBlock, bool interpolate)
        {
            feedback = Math.Max(Math.Min(feedback, 1.0f), -1.0f);//just to stop things getting accidentally out of control...
            delaySamples = Math.Abs(delaySamples);//should this be clamped to max 8..?
            int delaySamplesInt = (int)delaySamples;
            float frac = delaySamples - (float)Math.Floor(delaySamples);
            for (int i = 0; i < BufferSize; i++)
            {
                float currSample = buffer[i];
                float delayedSample;
                int delayed = i + delaySamplesInt;
                float s0 = stateBlock[delayed & 7];
                if (interpolate)
                {
                    float s1 = stateBlock[(delayed + 1) & 7];
                    delayedSample = (s1 - s0) * frac + s0;
                }
                else
                {
                    delayedSample = s0;
                }
                buffer[i] = delayedSample;
                stateBlock[i & 7] = delayedSample * feedback + currSample;
            }
        }

        private void AWFilter(float[] buffer, float[] buffer2, float[] buffer3, ref StateBlock stateBlock)
        {
            var s = stateBlock.X;
            for (int i = 0; i < BufferSize; i++)
            {
                var v10 = buffer3[i] * 64.0f;
                var v12 = (float)rnd.NextDouble() * v10 + buffer2[i] * 127.0f + 1.0f;
                var v13 = 1.0f / v12;
                var v14 = (1.0f - (v13 * v12)) * v13 + v13;
                s = s - ((s - buffer[i]) * v14);
                buffer[i] = s;
            }
            stateBlock.X = s;
        }

        // https://thewolfsound.com/allpass-filter/#first-order-iir-allpass
        private void AllpassFilter(float[] buffer, float breakFrequency, ref StateBlock stateBlock)
        {
            float previousUnfilteredSample = stateBlock.X;
            float previousSample = stateBlock.Y;

            float a1 = (float)Math.Tan(Math.PI * breakFrequency / SampleRate);
            a1 = (a1 - 1.0f) / (a1 + 1.0f);

            for (int i = 0; i < BufferSize; i++)
            {
                float sample = a1 * buffer[i] + previousUnfilteredSample - a1 * previousSample;
                sample = Math.Max(Math.Min(sample, 1.0f), -1.0f);

                previousUnfilteredSample = buffer[i];
                previousSample = sample;
                buffer[i] = sample;
            }

            stateBlock.X = previousUnfilteredSample;
            stateBlock.Y = previousSample;
        }

        private void AllpassFilter(float[] buffer, float[] breakFrequencies, ref StateBlock stateBlock)
        {
            float previousUnfilteredSample = stateBlock.X;
            float previousSample = stateBlock.Y;

            for (int i = 0; i < BufferSize; i++)
            {
                float a1 = (float)Math.Tan(Math.PI * breakFrequencies[i] / SampleRate);
                a1 = (a1 - 1.0f) / (a1 + 1.0f);

                float sample = a1 * buffer[i] + previousUnfilteredSample - a1 * previousSample;
                sample = Math.Max(Math.Min(sample, 1.0f), -1.0f);

                previousUnfilteredSample = buffer[i];
                previousSample = sample;
                buffer[i] = sample;
            }

            stateBlock.X = previousUnfilteredSample;
            stateBlock.Y = previousSample;
        }



        public struct StateBlock
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
            public float W { get; set; }
            public float A { get; set; }
            public float B { get; set; }
            public float C { get; set; }
            public float D { get; set; }

            public float this[int index]
            {
                get 
                {
                    switch (index)
                    {
                        default:
                        case 0: return X;
                        case 1: return Y;
                        case 2: return Z;
                        case 3: return W;
                        case 4: return A;
                        case 5: return B;
                        case 6: return C;
                        case 7: return D;
                    }
                }
                set
                {
                    switch (index)
                    {
                        default:
                        case 0: X = value; break;
                        case 1: Y = value; break;
                        case 2: Z = value; break;
                        case 3: W = value; break;
                        case 4: A = value; break;
                        case 5: B = value; break;
                        case 6: C = value; break;
                        case 7: D = value; break;
                    }
                }
            }

            public float[] Values
            {
                get
                {
                    return new[] { X, Y, Z, W, A, B, C, D };
                }
            }
        }
    }

}