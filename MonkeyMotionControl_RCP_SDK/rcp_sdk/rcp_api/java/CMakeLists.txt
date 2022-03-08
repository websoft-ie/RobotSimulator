#
# This CMAKE project will build a test application to exercise the
# RCP API.  It should compile and run on Cygwin (for Windows), OS X,
# and Linux.
#

set(CMAKE_LEGACY_CYGWIN_WIN32 0)

cmake_minimum_required(VERSION 2.6)

set(RCP_SDK ..)

set(VERSION_STRING $ENV{VERSION_STRING}) 
if(NOT VERSION_STRING)
    set(VERSION_STRING "local_build") 
endif(NOT VERSION_STRING) 



find_package(Doxygen)

if(DOXYGEN_FOUND)
    configure_file(${CMAKE_CURRENT_SOURCE_DIR}/Doxyfile.in ${CMAKE_CURRENT_BINARY_DIR}/Doxyfile @ONLY)
    configure_file(${CMAKE_CURRENT_SOURCE_DIR}/custom.css.in ${CMAKE_CURRENT_BINARY_DIR}/custom.css @ONLY)
    add_custom_target(doc
        ${DOXYGEN_EXECUTABLE} ${CMAKE_CURRENT_BINARY_DIR}/Doxyfile
        WORKING_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}
        COMMENT "Generating API documentation with Doxygen" VERBATIM
    )
endif(DOXYGEN_FOUND)
