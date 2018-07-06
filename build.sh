#! /bin/sh

ROOT_DIR=`pwd`
BUILD_DIR=`echo ${ROOT_DIR}/build`
DIST_DIR=`echo ${ROOT_DIR}/dist`
SRC_DIR=`echo ${ROOT_DIR}/src`
OBJ_DIR=`echo ${ROOT_DIR}/obj`
PROFILER_DIR=`echo ${SRC_DIR}/profiler`
CORECLR_DIR=`echo ${SRC_DIR}/coreclr`

echo "ROOT_DIR=${ROOT_DIR}"
echo "BUILD_DIR=${BUILD_DIR}"
echo "SRC_DIR=${SRC_DIR}"

mkdir -p ${OBJ_DIR}/profiler && cd ${OBJ_DIR}/profiler
cmake -DCMAKE_BUILD_TYPE=Release ${PROFILER_DIR}
make

cd ${ROOT_DIR}
rm -rf ${OBJ_DIR}