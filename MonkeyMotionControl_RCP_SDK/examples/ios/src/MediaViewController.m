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

#import "MediaViewController.h"
#import "RCPHandler.h"
#import "Clip.h"
#import "Logger.h"

@interface MediaViewController ()

@property (weak, nonatomic) IBOutlet UITableView *clipTable;
@property (weak, nonatomic) IBOutlet UILabel *unavailableLabel;
@property (weak, nonatomic) IBOutlet UIActivityIndicatorView *loadingIndicator;

@property (retain, nonatomic) NSMutableArray *clipList; // holds Clip objects


- (void)viewDidLoad;
- (void)didReceiveMemoryWarning;

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView;
- (NSInteger)tableView:(UITableView*)tableView numberOfRowsInSection:(NSInteger)section;
- (UITableViewCell*)tableView:(UITableView*)tableView cellForRowAtIndexPath:(NSIndexPath*)indexPath;

@end


@implementation MediaViewController

- (void)setConnected:(BOOL)connected
{
    if (!connected)
    {
        [_loadingIndicator setHidden:YES];
        [_clipTable setHidden:YES];
        [_unavailableLabel setHidden:NO];
        [_loadingIndicator stopAnimating];
    }
    
    [self.view setUserInteractionEnabled:connected];
    [self.navigationController.tabBarItem setEnabled:connected];
}

- (void)updateClipList:(rcp_clip_info_list_t *)clipList status:(rcp_clip_list_status_t)status
{
    switch (status)
    {
        case CLIP_LIST_LOADING:
            [_loadingIndicator startAnimating];
            [_loadingIndicator setHidden:NO];
            [_clipTable setHidden:YES];
            [_unavailableLabel setHidden:YES];
            break;
            
        case CLIP_LIST_DONE:
        {
            const rcp_clip_info_list_t *cur = clipList;
            [_clipList removeAllObjects];
            
            while (cur)
            {
                Clip *clip = [[Clip alloc] init];
                clip.index = cur->info.index;
                clip.name = [NSString stringWithFormat:@"%s", cur->info.clip_name];
                clip.date = [NSString stringWithFormat:@"%s", cur->info.clip_date];
                clip.time = [NSString stringWithFormat:@"%s", cur->info.clip_time];
                
                [_clipList addObject:clip];
                
                cur = cur->next;
            }
            
            [_clipTable reloadData];
            
            [_loadingIndicator setHidden:YES];
            [_clipTable setHidden:NO];
            [_unavailableLabel setHidden:YES];
            [_loadingIndicator stopAnimating];
            break;
        }
            
        case CLIP_LIST_BLOCKED:
            [_loadingIndicator setHidden:YES];
            [_clipTable setHidden:YES];
            [_unavailableLabel setHidden:NO];
            [_loadingIndicator stopAnimating];
            break;
            
        default:
            break;
    }
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    _clipList = [[NSMutableArray alloc] init];
    
    if ([_clipTable respondsToSelector:@selector(setSeparatorInset:)])
    {
        [_clipTable setSeparatorInset:UIEdgeInsetsZero];
    }
    
    [_clipTable setDelegate:self];
    [_clipTable setDataSource:self];
    
    [self setConnected:NO];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView
{
    return 1;
}

- (NSInteger)tableView:(UITableView*)tableView numberOfRowsInSection:(NSInteger)section
{
    return [_clipList count];
}

- (UITableViewCell*)tableView:(UITableView*)tableView cellForRowAtIndexPath:(NSIndexPath*)indexPath
{
    Clip *clip = [_clipList objectAtIndex:indexPath.row];
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:@"Clip"];
    
    [cell setSelectionStyle:UITableViewCellSelectionStyleNone];
    
    [cell.textLabel setText:[NSString stringWithFormat:@"%3d\t\t%@", clip.index, clip.name]];
    [cell.detailTextLabel setText:[NSString stringWithFormat:@"   \t\t%@ %@", clip.date, clip.time]];
    
    return cell;
}

@end
