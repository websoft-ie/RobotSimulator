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

#import "MenuListView.h"
#import "RCPHandler.h"

#define MAX_ROW_COUNT 13

@interface MenuListView()

@property (retain, nonatomic) UIView *container;
@property (retain, nonatomic) UITableView *listTable;
@property (retain, nonatomic) UIButton *doneButton;


- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView;
- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section;
- (UITableViewCell *)tableView:(UITableView*)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath;
- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath;

- (BOOL)tableView:(UITableView *)tableView canMoveRowAtIndexPath:(NSIndexPath *)indexPath;
- (UITableViewCellEditingStyle)tableView:(UITableView *)tableView editingStyleForRowAtIndexPath:(NSIndexPath *)indexPath;
- (BOOL)tableView:(UITableView *)tableview shouldIndentWhileEditingRowAtIndexPath:(NSIndexPath *)indexPath;
- (void)tableView:(UITableView *)tableView moveRowAtIndexPath:(NSIndexPath *)sourceIndexPath toIndexPath:(NSIndexPath *)destinationIndexPath;

- (void)donePressed;

@end


@implementation MenuListView

@synthesize delegate, rcpList;

- (MenuListView *)init
{
    if (self = [super init])
    {
        delegate = nil;
        rcpList = nil;
        
        _container = [[UIView alloc] init];
        _listTable = [[UITableView alloc] init];
        _doneButton = [[UIButton alloc] init];
        
        [_listTable setDelegate:self];
        [_listTable setDataSource:self];
        
        [_container addSubview:_listTable];
        [_container addSubview:_doneButton];
        [self addSubview:_container];
        
        
        [self setBackgroundColor:[UIColor colorWithRed:0.0 green:0.0 blue:0.0 alpha:0.4]];
        
        [_listTable setBackgroundColor:[UIColor colorWithRed:35.0/255.0 green:34.0/255.0 blue:38.0/255.0 alpha:1.0]];
        [_listTable setEditing:YES];
        
        if ([_listTable respondsToSelector:@selector(setSeparatorInset:)])
        {
            [_listTable setSeparatorInset:UIEdgeInsetsZero];
        }
        
        [_doneButton setBackgroundColor:[UIColor colorWithRed:65.0/255.0 green:64.0/255.0 blue:68.0/255.0 alpha:1.0]];
        [_doneButton.titleLabel setTextAlignment:NSTextAlignmentCenter];
        [_doneButton setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
        [_doneButton setTitleColor:[UIColor lightGrayColor] forState:UIControlStateHighlighted];
        [_doneButton setTitle:@"Done" forState:UIControlStateNormal];
        
        [_doneButton addTarget:self action:@selector(donePressed) forControlEvents:UIControlEventTouchUpInside];
    }
    
    return self;
}

- (void)setRcpList:(RCPList *)list
{
    rcpList = list;
    
    const CGFloat rowHeight = 44.0;
    const NSUInteger rowCount = [rcpList count];
    
    const CGFloat tableHeight = (rowHeight * ((rowCount > MAX_ROW_COUNT) ? MAX_ROW_COUNT : rowCount)) - 1.0;
    const CGFloat buttonHeight = rowHeight;
    const CGFloat containerWidth = self.frame.size.width * 0.9;
    const CGFloat containerHeight = tableHeight + buttonHeight;
    
    
    [_container setFrame:CGRectMake((self.frame.size.width - containerWidth) / 2.0, (self.frame.size.height - containerHeight) / 2.0, containerWidth, containerHeight)];
    [_listTable setFrame:CGRectMake(0.0, 0.0, containerWidth, tableHeight)];
    [_doneButton setFrame:CGRectMake(_listTable.frame.origin.x, _listTable.frame.origin.y + _listTable.frame.size.height, _listTable.frame.size.width, buttonHeight)];
    
    [_listTable reloadData];
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView
{
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section
{
    return [rcpList count];
}

- (UITableViewCell *)tableView:(UITableView*)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
    static NSString *cellIdentifier = @"ListEntry";
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:cellIdentifier];
    
    if (cell == nil)
    {
        cell = [[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:cellIdentifier];
        
        [cell setBackgroundColor:[UIColor clearColor]];
        [cell.contentView setBackgroundColor:[UIColor clearColor]];
        [cell setTintColor:[UIColor whiteColor]];
        [cell.textLabel setTextColor:[UIColor whiteColor]];
        
        [cell setShowsReorderControl:YES];
    }
    
    if (rcpList != nil)
    {
        if (indexPath.row < [rcpList count])
        {
            RCPListItem *item = [rcpList itemAtIndex:indexPath.row];
            [cell.textLabel setText:item.str];
        }
    }
    
    return cell;
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath
{
    [tableView deselectRowAtIndexPath:indexPath animated:YES];
}

- (BOOL)tableView:(UITableView *)tableView canMoveRowAtIndexPath:(NSIndexPath *)indexPath
{
    return YES;
}

- (UITableViewCellEditingStyle)tableView:(UITableView *)tableView editingStyleForRowAtIndexPath:(NSIndexPath *)indexPath
{
    return UITableViewCellEditingStyleNone;
}

- (BOOL)tableView:(UITableView *)tableview shouldIndentWhileEditingRowAtIndexPath:(NSIndexPath *)indexPath
{
    return NO;
}

- (void)tableView:(UITableView *)tableView moveRowAtIndexPath:(NSIndexPath *)sourceIndexPath toIndexPath:(NSIndexPath *)destinationIndexPath
{
    RCPListItem *itemToMove = [rcpList itemAtIndex:sourceIndexPath.row];
    
    [rcpList removeItemAtIndex:sourceIndexPath.row];
    [rcpList insertItem:itemToMove atIndex:destinationIndexPath.row];
}

- (void)donePressed
{
    RCPHandler *handler = [RCPHandler instance];
    [handler rcpSetList:rcpList];
    
    if (delegate != nil)
    {
        [delegate menuListViewFinishedEditing:self];
    }
}

@end
