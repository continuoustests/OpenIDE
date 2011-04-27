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
mkdir $DEPLOYDIR/templates
mkdir $DEPLOYDIR/ContinuousTests
chmod +x $LIB/ContinuousTests/AutoTest.*.exe
chmod +x $LIB/ContinuousTests/ContinuousTests.exe

echo $BINARYDIR

xbuild OpenIDENet.sln /target:rebuild /property:OutDir=$BINARYDIR/;Configuration=Release;
xbuild OpenIDENet.CodeEngine.sln /target:rebuild /property:OutDir=$BINARYDIR/;Configuration=Release;

cp $BINARYDIR/Castle.* $DEPLOYDIR/
cp $BINARYDIR/oi.exe $DEPLOYDIR/
cp $BINARYDIR/OpenIDENet.dll $DEPLOYDIR/
cp $ROOT/initialize.rb $DEPLOYDIR/initialize.rb
cp -r $LIB/EditorEngine/* $DEPLOYDIR/EditorEngine
cp -r $LIB/AutoTest.Net/* $DEPLOYDIR/AutoTest.Net
cp -r $LIB/ContinuousTests/* $DEPLOYDIR/ContinuousTests
cp -r $ROOT/templates/* $DEPLOYDIR/templates
cp $ROOT/oi/oi $DEPLOYDIR/oi
cp $BINARYDIR/OpenIDENet.CodeEngine.exe $DEPLOYDIR/CodeEngine/OpenIDENet.CodeEngine.exe
cp $BINARYDIR/OpenIDENet.CodeEngine.Core.dll $DEPLOYDIR/CodeEngine/OpenIDENet.CodeEngine.Core.dll

