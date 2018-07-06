#! /bin/sh

ROOT_DIR=`pwd`
BUILD_DIR=`echo ${ROOT_DIR}/build`
DIST_DIR=`echo ${ROOT_DIR}/dist`
SRC_DIR=`echo ${ROOT_DIR}/src`
OBJ_DIR=`echo ${ROOT_DIR}/obj`
PROFILER_DIR=`echo ${SRC_DIR}/profiler`
CORECLR_DIR=`echo ${SRC_DIR}/coreclr`

BUILD_TYPE="debug"
BUILD_CORECLR=false

while [ $# != 0 ]; do
    case $1 in
        debug | -debug)
            BUILD_TYPE=$1
        ;;
        release | -release)
            BUILD_TYPE=$1
        ;;
        build_coreclr | -build_coreclr)
            BUILD_CORECLR=true
        ;;
        esac
    shift
done

echo "ROOT_DIR=${ROOT_DIR}"
echo "BUILD_DIR=${BUILD_DIR}"
echo "SRC_DIR=${SRC_DIR}"

if [ $BUILD_CORECLR == true ]
then
cd ${CORECLR_DIR}
./build.sh ${BUILD_TYPE}
fi

cd ${ROOT_DIR}
mkdir -p ${OBJ_DIR}/profiler && cd ${OBJ_DIR}/profiler
cmake -DCMAKE_BUILD_TYPE=${BUILD_TYPE} ${PROFILER_DIR}
make

cd ${ROOT_DIR}
rm -rf ${OBJ_DIR}