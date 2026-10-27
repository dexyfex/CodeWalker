#!/usr/bin/env bash
# The checks CI runs, in the order CI runs them. Usage: scripts/check.sh [cargo package args]
# e.g. `scripts/check.sh -p cw-core -p cw-cmd`; with no arguments it checks the whole workspace.
# Fails on the first problem. If a newer toolchain is installed side by side (`rustup toolchain install
# 1.99.1`), clippy also runs on it, because CI's lint job uses a newer clippy than the pinned minimum.
set -uo pipefail
cd "$(dirname "$0")/.."

pkgs=("$@")
[ ${#pkgs[@]} -eq 0 ] && pkgs=(--workspace)

run() {
  local label=$1
  shift
  local out
  if ! out=$("$@" 2>&1); then
    echo "FAILED: $label"
    echo "$out" | grep -E "^(error|warning)|-->|^test .*FAILED|panicked|^failures:" -A6 | head -60
    exit 1
  fi
  echo "ok: $label"
}

run "fmt" cargo fmt --check
run "clippy" cargo clippy "${pkgs[@]}" --all-targets -- -D warnings
if rustup toolchain list 2>/dev/null | grep -q '^1\.99\.1'; then
  run "clippy (1.99.1)" cargo +1.99.1 clippy "${pkgs[@]}" --all-targets -- -D warnings
fi
run "tests" cargo test "${pkgs[@]}"
