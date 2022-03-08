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

#import "TabViewController.h"
#import "ConnectionViewController.h"
#import "CameraViewController.h"
#import "MediaViewController.h"
#import "MenuTree.h"
#import "NotificationView.h"
#import "RCPHandler.h"
#import "RCPUtil.h"
#import "Logger.h"

@interface TabViewController ()

@property (assign, nonatomic) ConnectionViewController *connectionVC;
@property (assign, nonatomic) CameraViewController *cameraVC;
@property (assign, nonatomic) MediaViewController *mediaVC;
@property (retain, nonatomic) MenuTree *menuTree;

@property (retain, nonatomic) NotificationView *notificationView;
@property (retain, nonatomic) UIButton *menuTreeButton;
@property (assign, nonatomic) playback_state_t playbackState;


- (id)initWithCoder:(NSCoder *)aDecoder;
- (void)viewDidLoad;
- (void)didReceiveMemoryWarning;

- (void)keyboardWillShow:(NSNotification*)notification;
- (void)keyboardWillHide:(NSNotification*)notification;

- (void)requestMenuTree;

@end


@implementation TabViewController

static TabViewController *_instance;

+ (TabViewController *)instance
{
    return _instance;
}

- (void)setConnected:(BOOL)connected
{
    [_connectionVC setConnected:connected];
    [_cameraVC setConnected:connected];
    [_mediaVC setConnected:connected];
    
    if (connected)
    {
        RCPHandler *handler = [RCPHandler instance];
        const BOOL menuSupported = [handler rcpMenuIsSupported];
        
        [handler rcpGet:RCP_PARAM_PLAYBACK_STATE];
        
        [self setSelectedIndex:1];
        [_menuTreeButton setHidden:!menuSupported];
        [_menuTreeButton setEnabled:menuSupported];
        
        [_connectionVC.navigationItem setTitle:handler.connectedCamera.cameraID];
        [_cameraVC.navigationItem setTitle:handler.connectedCamera.cameraID];
        [_mediaVC.navigationItem setTitle:handler.connectedCamera.cameraID];
    }
    else
    {
        [self setSelectedIndex:0];
        [_menuTree hide];
        [_menuTreeButton setHidden:YES];
        [_menuTreeButton setEnabled:NO];
        
        [_connectionVC.navigationItem setTitle:@"Connection"];
        [_cameraVC.navigationItem setTitle:@"Connection"];
        [_mediaVC.navigationItem setTitle:@"Connection"];
    }
}

- (void)updateParam:(rcp_param_t)paramID integer:(const int)val infoIsValid:(BOOL)infoIsValid info:(rcp_cur_int_edit_info_t)editInfo
{
    if (paramID == RCP_PARAM_PLAYBACK_STATE && val != _playbackState)
    {
        _playbackState = val;
        [_menuTree hide];
    }
    
    [_cameraVC updateParam:paramID integer:val infoIsValid:infoIsValid info:editInfo];
    [_menuTree updateParam:paramID integer:val infoIsValid:infoIsValid info:editInfo];
}

- (void)updateParam:(rcp_param_t)paramID uinteger:(const uint32_t)val infoIsValid:(BOOL)infoIsValid info:(rcp_cur_uint_edit_info_t)editInfo
{
    [_menuTree updateParam:paramID uinteger:val infoIsValid:infoIsValid info:editInfo];
}

- (void)updateParam:(rcp_param_t)paramID string:(const char *)str status:(rcp_param_status_t)status
{
    if (paramID == RCP_PARAM_CAMERA_ID)
    {
        RCPHandler *handler = [RCPHandler instance];
        [handler.connectedCamera setCameraID:[NSString stringWithFormat:@"%s", str]];
        
        [_connectionVC.navigationItem setTitle:handler.connectedCamera.cameraID];
        [_cameraVC.navigationItem setTitle:handler.connectedCamera.cameraID];
        [_mediaVC.navigationItem setTitle:handler.connectedCamera.cameraID];
    }
    
    [_cameraVC updateParam:paramID string:str status:status];
    [_menuTree updateParam:paramID string:str status:status];
}

- (void)updateParam:(rcp_param_t)paramID string:(const char *)str status:(rcp_param_status_t)status infoIsValid:(BOOL)infoIsValid info:(rcp_cur_str_edit_info_t)editInfo
{
    if (paramID == RCP_PARAM_CAMERA_ID)
    {
        RCPHandler *handler = [RCPHandler instance];
        [handler.connectedCamera setCameraID:[NSString stringWithFormat:@"%s", str]];
        
        [_connectionVC.navigationItem setTitle:handler.connectedCamera.cameraID];
        [_cameraVC.navigationItem setTitle:handler.connectedCamera.cameraID];
        [_mediaVC.navigationItem setTitle:handler.connectedCamera.cameraID];
    }
    
    [_cameraVC updateParam:paramID string:str status:status infoIsValid:infoIsValid info:editInfo];
    [_menuTree updateParam:paramID string:str status:status infoIsValid:infoIsValid info:editInfo];
}

- (void)updateParam:(rcp_param_t)paramID list:(RCPList *)list
{
    [_cameraVC updateParam:paramID list:list];
    [_menuTree updateParam:paramID list:list];
}

- (void)updateParam:(rcp_param_t)paramID frameTagInfo:(tag_info_t)tagInfo
{
    [_cameraVC updateParam:paramID frameTagInfo:tagInfo];
}

- (void)updateParam:(rcp_param_t)paramID enabled:(BOOL)enabled
{
    [_cameraVC updateParam:paramID enabled:enabled];
    [_menuTree updateParam:paramID enabled:enabled];
}

- (void)updateNode:(rcp_menu_node_id_t)nodeID enabled:(BOOL)enabled
{
    [_menuTree updateNode:nodeID enabled:enabled];
}

- (void)updateClipList:(rcp_clip_info_list_t *)clipList status:(rcp_clip_list_status_t)status
{
    [_mediaVC updateClipList:clipList status:status];
}

- (void)updateHistogram:(const rcp_cur_hist_cb_data_t *)data
{
    [_cameraVC updateHistogram:data];
}

- (void)updateAudioVU:(const rcp_cur_audio_vu_cb_data_t *)data
{
    // Implementation is empty on purpose; no parameters in this app utilize the audio VU callback
}

- (void)updateMenuTree:(const rcp_cur_menu_cb_data_t *)data
{
    if (_menuTree.menuTreeRequested)
    {
        [_menuTree setMenuData:data];
        
        if (![_menuTree isVisible])
        {
            [_menuTree show];
        }
        
        _menuTree.menuTreeRequested = NO;
    }
}

- (void)showNotificationView:(Notification *)notification
{
    _notificationView.notification = notification;
    [_notificationView showInView:self.view];
}

- (void)updateNotificationView:(Notification *)notification
{
    _notificationView.notification = notification;
    [_notificationView update];
}

- (void)hideNotificationView
{
    _notificationView.notification = nil;
    [_notificationView hide];
}

- (void)enableIOSNotifications:(BOOL)enable
{
    if (enable)
    {
        // Ensures that notifications are not enabled more than once
        [self enableIOSNotifications:NO];
        
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(keyboardWillShow:) name:UIKeyboardWillShowNotification object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(keyboardWillHide:) name:UIKeyboardWillHideNotification object:nil];
    }
    else
    {
        [[NSNotificationCenter defaultCenter] removeObserver:self name:UIKeyboardWillShowNotification object:nil];
        [[NSNotificationCenter defaultCenter] removeObserver:self name:UIKeyboardWillHideNotification object:nil];
    }
}

- (id)initWithCoder:(NSCoder *)aDecoder
{
    if (self = [super initWithCoder:aDecoder])
    {
        _instance = self;
    }
    
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    _connectionVC = (ConnectionViewController *)[[[self viewControllers] objectAtIndex:0] topViewController];
    _cameraVC = (CameraViewController *)[[[self viewControllers] objectAtIndex:1] topViewController];
    _mediaVC = (MediaViewController *)[[[self viewControllers] objectAtIndex:2] topViewController];
    _menuTree = [[MenuTree alloc] init];
    
    _notificationView = [[NotificationView alloc] init];
    _menuTreeButton = [[UIButton alloc] init];
    _playbackState = PLAYBACK_STATE_NOT_IN_PLAYBACK;
    
    
    NSString *menuButtonTitle = @"Menu";
    UIFont *menuButtonFont = [UIFont systemFontOfSize:16.0];
    const CGSize menuButtonSize = [RCPUtil sizeOfText:menuButtonTitle font:menuButtonFont];
    
    [_menuTreeButton.titleLabel setFont:menuButtonFont];
    [_menuTreeButton setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
    [_menuTreeButton setTitleColor:[UIColor colorWithWhite:1.0 alpha:0.5] forState:UIControlStateHighlighted];
    [_menuTreeButton setTitle:menuButtonTitle forState:UIControlStateNormal];
    [_menuTreeButton addTarget:self action:@selector(requestMenuTree) forControlEvents:UIControlEventTouchUpInside];
    [_menuTreeButton setFrame:CGRectMake(self.view.bounds.size.width - menuButtonSize.width - 10.0, 35.0, menuButtonSize.width, menuButtonSize.height)];
    
    [self.view addSubview:_menuTreeButton];
    [[self tabBar] setTintColor:[UIColor colorWithRed:240.0/255.0 green:240.0/255.0 blue:240.0/255.0 alpha:1.0]];
    [self enableIOSNotifications:YES];
    
    // Pre-loads each view
    [_connectionVC view];
    [_cameraVC view];
    [_mediaVC view];
    
    [self setConnected:NO];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
}

- (void)keyboardWillShow:(NSNotification*)notification
{
    CGRect keyboardFrame = [[[notification userInfo] objectForKey:UIKeyboardFrameBeginUserInfoKey] CGRectValue];
    UIViewController *activeTab = [[self.viewControllers objectAtIndex:[self selectedIndex]] topViewController];
    
    if ([_menuTree isVisible])
    {
        [_menuTree keyboardWillShow:keyboardFrame];
    }
    else
    {
        if ([activeTab isKindOfClass:[ConnectionViewController class]])
        {
            [_connectionVC keyboardWillShow:keyboardFrame];
        }
        else if ([activeTab isKindOfClass:[CameraViewController class]])
        {
            [_cameraVC keyboardWillShow:keyboardFrame];
        }
    }
}

- (void)keyboardWillHide:(NSNotification*)notification
{
    CGRect keyboardFrame = [[[notification userInfo] objectForKey:UIKeyboardFrameBeginUserInfoKey] CGRectValue];
    UIViewController *activeTab = [[self.viewControllers objectAtIndex:[self selectedIndex]] topViewController];
    
    if ([_menuTree isVisible])
    {
        [_menuTree keyboardWillHide:keyboardFrame];
    }
    else
    {
        if ([activeTab isKindOfClass:[ConnectionViewController class]])
        {
            [_connectionVC keyboardWillHide:keyboardFrame];
        }
        else if ([activeTab isKindOfClass:[CameraViewController class]])
        {
            [_cameraVC keyboardWillHide:keyboardFrame];
        }
    }
}

- (void)requestMenuTree
{
    if ([_menuTree isVisible])
    {
        [_menuTree hide];
    }
    else
    {
        _menuTree.menuTreeRequested = YES;
        
        RCPHandler *handler = [RCPHandler instance];
        [handler rcpGetMenu:RCP_MENU_NODE_ID_ROOT];
    }
}

@end
