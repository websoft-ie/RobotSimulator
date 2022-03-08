/********************************************************************************
 * This file is part of the RCP SDK Release 6.62.0
 * Copyright (C) 2009-2018 RED.COM, LLC.  All rights reserved.
 *
 * For technical support please email rcpsdk@red.com.
 *
 * "Source Code" means the accompanying software in any form preferred for making
 * modifications. "Source Code" does not include the accompanying strlcat.c and
 * strlcpy.c software and examples/qt/common/qt/serial software.
 * 
 * "Binary Code" means machine-readable Source Code in binary form.
 * 
 * "Approved Recipients" means only those recipients of the Source Code who have
 * entered into the RCP SDK License Agreement with RED.COM, LLC. All
 * other recipients are not authorized to possess, modify, use, or distribute the
 * Source Code.
 *
 * RED.COM, LLC hereby grants Approved Recipients the rights to modify this
 * Source Code, create derivative works based on this Source Code, and distribute
 * the modified/derivative works only as Binary Code in its binary form. Approved
 * Recipients may not distribute the Source Code or any modification or derivative
 * work of the Source Code. Redistributions of Binary Code must reproduce this
 * copyright notice, this list of conditions, and the following disclaimer in the
 * documentation or other materials provided with the distribution. RED.COM, LLC
 * may not be used to endorse or promote Binary Code redistributions without
 * specific prior written consent from RED.COM, LLC. 
 *
 * The only exception to the above licensing requirements is any recipient may use,
 * copy, modify, and distribute in any format the strlcat.c and strlcpy.c software
 * in accordance with the provisions required by the license associated therewith;
 * provided, however, that the modifications are solely to the strlcat.c and
 * strlcpy.c software and do not include any other portion of the Source Code.
 * 
 * THE ACCOMPANYING SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, TITLE, AND NON-INFRINGEMENT.
 * IN NO EVENT SHALL THE RED.COM, LLC, ANY OTHER COPYRIGHT HOLDERS, OR ANYONE
 * DISTRIBUTING THE SOFTWARE BE LIABLE FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER
 * IN CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 ********************************************************************************/

#import <UIKit/UIKit.h>
#import <sys/time.h>
#import "AppDelegate.h"
#import "Logger.h"
#import "rcp_api.h"

static NSRecursiveLock *rcp_mutex[RCP_MUTEX_COUNT];

void rcp_mutex_lock(rcp_mutex_t mutexID)
{
    [rcp_mutex[mutexID] lock];
}

void rcp_mutex_unlock(rcp_mutex_t mutexID)
{
    [rcp_mutex[mutexID] unlock];
}

void *rcp_malloc(size_t NBYTES)
{
    return malloc(NBYTES);
}

void rcp_free(void *APTR)
{
    free(APTR);
}

void rcp_log(rcp_log_t severity, const rcp_camera_connection_t *con, const char *msg)
{
     switch (severity)
     {
         case RCP_LOG_ERROR:
             LogError("%s", msg);
             break;
         
         case RCP_LOG_WARNING:
             LogWarning("%s", msg);
             break;
         
         case RCP_LOG_INFO:
             LogInfo("%s", msg);
             break;
         
         case RCP_LOG_DEBUG:
             LogDebug("%s", msg);
             break;
         
         default:
             break;
     }
}

int rcp_rand(void)
{
    static int seed = 1;
    
    if (seed)
    {
        srand((unsigned int)time(NULL));
        seed = 0;
    }
    
    return rand();
}

uint32_t rcp_timestamp(void)
{
    uint32_t ts;
    struct timeval tv;
    gettimeofday(&tv, NULL);
    ts = (uint32_t)((tv.tv_sec * 1000) + (tv.tv_usec / 1000));
    return ts;
}

int main(int argc, char * argv[])
{
    int i;
    for (i = 0; i < RCP_MUTEX_COUNT; i++)
    {
        rcp_mutex[i] = [[NSRecursiveLock alloc] init];
    }
    
    @autoreleasepool {
        return UIApplicationMain(argc, argv, nil, NSStringFromClass([AppDelegate class]));
    }
}
