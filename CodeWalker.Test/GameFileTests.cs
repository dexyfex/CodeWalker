using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CodeWalker.Test
{
    public class GameFileTests
    {
        [Fact]
        public void SettingLoadedShouldUpdateLoaded()
        {
            var gameFile = new YmapFile();

            gameFile.Loaded = true;

            Assert.True(gameFile.Loaded);

            gameFile.Loaded = false;

            Assert.False(gameFile.Loaded);
        }

        [Fact]
        public void SettingLoadQueuedShouldUpdateLoadQueued()
        {
            var gameFile = new YmapFile();

            gameFile.LoadQueued = true;

            Assert.True(gameFile.LoadQueued);

            gameFile.LoadQueued = false;

            Assert.False(gameFile.LoadQueued);
        }

        [Fact]
        // This is a requirement for thread safety, this allows an if statement which is atomic when checking for load state
        public void SetLoadQueuedShouldReturnFalseWhenValueNotUpdated()
        {
            var gameFile = new YmapFile();

            Assert.False(gameFile.LoadQueued);

            Assert.True(gameFile.SetLoadQueued(true));
            Assert.False(gameFile.SetLoadQueued(true));

            Assert.True(gameFile.SetLoadQueued(false));
            Assert.False(gameFile.SetLoadQueued(false));
        }

        [Fact]
        public void SetLoadedShouldReturnFalseWhenValueNotUpdated()
        {
            var gameFile = new YmapFile();

            Assert.False(gameFile.Loaded);

            Assert.True(gameFile.SetLoaded(true));
            Assert.False(gameFile.SetLoaded(true));

            Assert.True(gameFile.SetLoaded(false));
            Assert.False(gameFile.SetLoaded(false));
        }
    }
}
