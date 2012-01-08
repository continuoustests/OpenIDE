#!/bin/bash

ROOT=$(cd $(dirname "$0"); pwd)
BINARYDIR=$(cd $(dirname "$0"); pwd)/build_output
DEPLOYDIR=$(cd $(dirname "$0"); pwd)/ReleaseBinaries
LIB=$(cd $(dirname "$0"); pwd)/lib

if [ ! -d $BINARYDIR ]; then
{
	mkdir $BINARYDIR
}
fi
if [ ! -d $DEPLOYDIR ]; then
{
	mkdir $DEPLOYDIR
}
fi

rm -r $BINARYDIR/*
rm -r $DEPLOYDIR/*
mkdir $DEPLOYDIR/EditorEngine
mkdir $DEPLOYDIR/CodeEngine
mkdir $DEPLOYDIR/AutoTest.Net
mkdir $DEPLOYDIR/ContinuousTests
chmod +x $LIB/ContinuousTests/AutoTest.*.exe
chmod +x $LIB/ContinuousTests/ContinuousTests.exe

mkdir $DEPLOYDIR/Languages
mkdir $DEPLOYDIR/Languages/CSharp

echo $BINARYDIR

xbuild OpenIDENet.sln /target:rebuild /property:OutDir=$BINARYDIR/;Configuration=Release;
xbuild OpenIDENet.CodeEngine.sln /target:rebuild /property:OutDir=$BINARYDIR/;Configuration=Release;
xbuild Languages/CSharp/CSharp.sln /target:rebuild /property:OutDir=$BINARYDIR/;Configuration=Release;

cp $BINARYDIR/oi.exe $DEPLOYDIR/
cp $BINARYDIR/OpenIDENet.dll $DEPLOYDIR/
cp $BINARYDIR/OpenIDENet.Core.dll $DEPLOYDIR/
cp $ROOT/initialize.rb $DEPLOYDIR/initialize.rb
cp -r $LIB/EditorEngine/* $DEPLOYDIR/EditorEngine
cp -r $LIB/AutoTest.Net/* $DEPLOYDIR/AutoTest.Net
cp -r $LIB/ContinuousTests/* $DEPLOYDIR/ContinuousTests
cp $ROOT/oi/oi $DEPLOYDIR/oi
cp $BINARYDIR/OpenIDENet.CodeEngine.exe $DEPLOYDIR/CodeEngine/OpenIDENet.CodeEngine.exe
cp $BINARYDIR/OpenIDENet.CodeEngine.Core.dll $DEPLOYDIR/CodeEngine/OpenIDENet.CodeEngine.Core.dll
cp $BINARYDIR/OpenIDENet.Core.dll $DEPLOYDIR/CodeEngine/

cp $BINARYDIR/CSharp.exe $DEPLOYDIR/Languages/CSharp.exe
cp -r $ROOT/Languages/CSharp/templates/* $DEPLOYDIR/Languages/CSharp
