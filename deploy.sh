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
mkdir $DEPLOYDIR/AutoTest.Net
mkdir $DEPLOYDIR/templates
mkdir $DEPLOYDIR/ContinuousTests
chmod +x $LIB/ContinuousTests/AutoTest.VM.exe
chmod +x $LIB/ContinuousTests/ContinuousTests.exe

echo $BINARYDIR

xbuild OpenIDENet.sln /target:rebuild /property:OutDir=$BINARYDIR/;Configuration=Release;

cp $BINARYDIR/Castle.* $DEPLOYDIR/
cp $BINARYDIR/oi.exe $DEPLOYDIR/
cp $BINARYDIR/OpenIDENet.dll $DEPLOYDIR/
cp $ROOT/initialize.rb $DEPLOYDIR/initialize.rb
cp -r $LIB/EditorEngine/* $DEPLOYDIR/EditorEngine
cp -r $LIB/AutoTest.Net/* $DEPLOYDIR/AutoTest.Net
cp -r $LIB/ContinuousTests/* $DEPLOYDIR/ContinuousTests
cp -r $ROOT/templates/* $DEPLOYDIR/templates

