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

#import "NotificationView.h"
#import "RCPHandler.h"
#import "RCPUtil.h"
#import "Logger.h"

@interface NotificationView()

@property (nonatomic, retain) NSTimer *notificationTimer;
@property (nonatomic, retain) UIView *container;
@property (nonatomic, retain) UILabel *titleLabel;
@property (nonatomic, retain) UILabel *messageLabel;
@property (nonatomic, retain) UIProgressView *finiteProgressView;
@property (nonatomic, retain) UIActivityIndicatorView *infiniteProgressView;


- (void)timeout;
- (void)notificationButtonPressed:(NotificationButton *)button;

@end


@implementation NotificationView

@synthesize notification;

- (NotificationView *)init
{
    if (self = [super init])
    {
        notification = nil;
        
        _notificationTimer = nil;
        _container = [[UIView alloc] init];
        _titleLabel = [[UILabel alloc] init];
        _messageLabel = [[UILabel alloc] init];
        _finiteProgressView = [[UIProgressView alloc] init];
        _infiniteProgressView = [[UIActivityIndicatorView alloc] initWithActivityIndicatorStyle:UIActivityIndicatorViewStyleWhiteLarge];
        
        
        [self setBackgroundColor:[UIColor colorWithRed:0.0 green:0.0 blue:0.0 alpha:0.4]];
        
        [_container setBackgroundColor:[UIColor whiteColor]];
        [_container.layer setCornerRadius:7.0];
        
        [_titleLabel setFont:[UIFont boldSystemFontOfSize:20.0]];
        [_titleLabel setTextAlignment:NSTextAlignmentCenter];
        [_titleLabel setTextColor:[UIColor blackColor]];
        [_titleLabel setNumberOfLines:1];
        
        [_messageLabel setFont:[UIFont systemFontOfSize:16.0]];
        [_messageLabel setTextAlignment:NSTextAlignmentCenter];
        [_messageLabel setTextColor:[UIColor blackColor]];
        [_messageLabel setNumberOfLines:0]; // enables word wrapping
        
        [_infiniteProgressView setColor:[UIColor blackColor]];
        
        
        // Makes it so tapping the screen during a timeout-based notification dismisses it
        UITapGestureRecognizer *singleTap = [[UITapGestureRecognizer alloc] initWithTarget:self action:@selector(timeout)];
        [self addGestureRecognizer:singleTap];
        
        // Both the titleLabel and the messageLabel will always be present; the progress/infinite progress views will only conditionally be added
        [_container addSubview:_titleLabel];
        [_container addSubview:_messageLabel];
        
        [self addSubview:_container];
    }
    
    return self;
}

- (void)showInView:(UIView *)view
{
    if (self.superview == nil)
    {
        if (notification == nil)
        {
            LogError("Unable to show nil notification.");
        }
        else
        {
            [self setFrame:CGRectMake(0.0, 0.0, view.bounds.size.width, view.bounds.size.height)];
            [self update];
            [view addSubview:self];
            
            if (notification.timeout > 0)
            {
                _notificationTimer = [NSTimer scheduledTimerWithTimeInterval:notification.timeout target:self selector:@selector(timeout) userInfo:nil repeats:NO];
            }
        }
    }
}

- (void)update
{
    if (notification == nil)
    {
        LogError("Unable to update nil notification.");
    }
    else
    {
        // Remove all subviews except for the title and the message
        for (UIView *v in _container.subviews)
        {
            if (![v isKindOfClass:[UILabel class]])
            {
                [v removeFromSuperview];
            }
        }
        
        const CGFloat horizontalSpacing = 5.0;
        const CGFloat verticalSpacing = 5.0;
        const CGFloat containerWidth = 300.0;

        const CGSize titleSize = CGSizeMake(containerWidth - (horizontalSpacing * 2.0), [RCPUtil sizeOfText:notification.title font:_titleLabel.font].height);
        
        NSAttributedString *attributedMessage = [[NSAttributedString alloc] initWithString:notification.message attributes:@{NSFontAttributeName: _messageLabel.font}];
        const CGSize messageSize = CGSizeMake(containerWidth - (horizontalSpacing * 2.0), [attributedMessage boundingRectWithSize:CGSizeMake(containerWidth - (horizontalSpacing * 2.0), CGFLOAT_MAX) options:NSStringDrawingUsesLineFragmentOrigin context:nil].size.height);
        
        const CGSize finiteProgressSize = CGSizeMake(containerWidth - (horizontalSpacing * 2.0), 2.0);
        
        const CGSize infiniteProgressSize = CGSizeMake(37.0, 37.0);
        
        const CGSize buttonSize = CGSizeMake(containerWidth - (horizontalSpacing * 2.0), 40.0);
     
        CGFloat y = verticalSpacing;
        
        [_titleLabel setText:notification.title];
        [_titleLabel setFrame:CGRectMake(horizontalSpacing, y, titleSize.width, titleSize.height)];
        y += _titleLabel.bounds.size.height + verticalSpacing;
        
        [_messageLabel setText:notification.message];
        [_messageLabel setFrame:CGRectMake(horizontalSpacing, y, messageSize.width, messageSize.height)];
        y += _messageLabel.bounds.size.height + verticalSpacing;
        
        switch (notification.progressType)
        {
            case NOTIFICATION_PROGRESS_NONE:
                [_finiteProgressView setProgress:0.0];
                [_infiniteProgressView stopAnimating];
                break;
                
            case NOTIFICATION_PROGRESS_NORMAL:
                if (_finiteProgressView.superview == nil)
                {
                    [_container addSubview:_finiteProgressView];
                }
                
                [_infiniteProgressView stopAnimating];
                [_finiteProgressView setProgress:(notification.progressPercent / 100.0)];
                [_finiteProgressView setFrame:CGRectMake(horizontalSpacing, y, finiteProgressSize.width, finiteProgressSize.height)];
                y += _finiteProgressView.bounds.size.height + verticalSpacing;
                break;
                
            case NOTIFICATION_PROGRESS_INFINITE:
                if (_infiniteProgressView.superview == nil)
                {
                    [_container addSubview:_infiniteProgressView];
                }
                
                [_finiteProgressView setProgress:0.0];
                [_infiniteProgressView startAnimating];
                [_infiniteProgressView setFrame:CGRectMake((containerWidth - infiniteProgressSize.width) / 2.0, y, infiniteProgressSize.width, infiniteProgressSize.height)];
                y += _infiniteProgressView.bounds.size.height + verticalSpacing;
                break;
                
            default:
                break;
        }
        
        for (NotificationButton *button in notification.buttons)
        {
            [_container addSubview:button];
            
            if (button.delegate == nil)
            {
                [button setDelegate:self];
            }
            
            [button setTitle:button.name forState:UIControlStateNormal];
            [button setFrame:CGRectMake(horizontalSpacing, y, buttonSize.width, buttonSize.height)];
            y += button.bounds.size.height + verticalSpacing;
        }
        
        [_container setFrame:CGRectMake((self.bounds.size.width - containerWidth) / 2.0, (self.bounds.size.height - y) / 2.0, containerWidth, y)];
    }
}

- (void)hide
{
    if (self.superview != nil)
    {
        [self removeFromSuperview];
    }
}

- (void)timeout
{
    if (notification == nil)
    {
        LogError("Unable to timeout nil notification.");
    }
    else
    {
        [_notificationTimer invalidate];
        _notificationTimer = nil;
        
        RCPHandler *handler = [RCPHandler instance];
        [handler rcpNotificationTimeout:[notification.notificationID UTF8String]];
    }
}

- (void)notificationButtonPressed:(NotificationButton *)button
{
    RCPHandler *handler = [RCPHandler instance];
    [handler rcpNotificationResponse:[notification.notificationID UTF8String] response:button.value];
}

@end
