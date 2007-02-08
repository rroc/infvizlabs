#!/bin/sh

rm -rf lib; mkdir lib
mcs /target:library /out:lib/MusicBrainzDotNet.dll \
  src/BitprintInfo.cs \
  src/TRM.cs \
  src/MusicBrainz.cs

rm -rf bin; mkdir bin
mcs /target:exe /out:bin/getalbum.exe /r:lib/MusicBrainzDotNet.dll \
  examples/getalbum.cs
