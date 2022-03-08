LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE	:= rcp

LOCAL_LDLIBS := -llog -lz

LOCAL_CFLAGS := -g -DRCP_ANDROID 

LOCAL_CPPFLAGS := -g -fpermissive -DRCP_ANDROID

# RCP_CORE and RCP_API env variables are set by build_android.gradle file that launched this NDK build.
# They are derived from RCP_SDK global env variable.
LOCAL_C_INCLUDES := $(RCP_CORE) \
					$(RCP_API) \
					$(ANDROID_NDK)/sources/cxx-stl/gnu-libstdc++/4.6/include \
					$(ANDROID_NDK)/sources/cxx-stl/gnu-libstdc++/4.6/libs/armeabi-v7a/include \
					$(ANDROID_NDK)/sources/cxx-stl/stlport/stlport

LOCAL_SRC_FILES :=	com_red_rcp_RCPDiscovery.cpp \
					com_red_rcp_RCPCamera.cpp \
					rcp_common.cpp \
					rcp_discovery.cpp \
					rcp_camera.cpp

LOCAL_SRC_FILES +=  $(RCP_API)/rcp_api.c \
					$(RCP_CORE)/clist/clist.cpp

include $(BUILD_SHARED_LIBRARY)
