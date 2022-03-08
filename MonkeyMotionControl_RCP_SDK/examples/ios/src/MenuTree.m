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

#import "MenuTree.h"
#import "MenuNode.h"
#import "MenuTable.h"
#import "TabViewController.h"
#import "TextInputView.h"
#import "RCPHandler.h"
#import "RCPUtil.h"
#import "Logger.h"

#define MAX_ROW_COUNT 11
#define VIEW_WIDTH 320.0
#define VIEW_HEIGHT 568.0

@interface MenuTree()

@property (retain, nonatomic) NSMutableArray *childrenList;     // holds MenuNode objects
@property (retain, nonatomic) NSMutableArray *ancestorList;     // holds MenuNode objects

@property (retain, nonatomic) UIView *navigationView;
@property (retain, nonatomic) UIView *navigationBar;
@property (retain, nonatomic) UIButton *backButton;
@property (retain, nonatomic) MenuTable *menuTable;
@property (retain, nonatomic) MenuListView *orderedList;
@property (retain, nonatomic) CurveView *curveView;
@property (retain, nonatomic) TextInputView *textInputView;
@property (retain, nonatomic) MultiActionView *multiActionView;


- (void)navigateBack;
- (void)navigationButtonPressed:(UIButton *)sender;

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView;
- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section;
- (UITableViewCell *)tableView:(UITableView*)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath;
- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath;
- (NSIndexPath *)tableView:(UITableView *)tableView willSelectRowAtIndexPath:(NSIndexPath *)indexPath;

- (void)menuTableViewCellSelected:(MenuTableViewCell *)cell;
- (void)menuTableViewCell:(MenuTableViewCell *)cell textInputViewRequestedFromPickerControl:(PickerControl *)pickerControl;
- (void)menuTableViewCell:(MenuTableViewCell *)cell textInputViewRequestedFromTextControl:(TextControl *)textControl;

- (void)menuListViewFinishedEditing:(MenuListView *)menuListView;
- (void)curveViewFinishedEditing:(CurveView *)curveView;

- (void)multiActionViewReadyToOpen:(MultiActionView *)multiActionView;

- (void)constructNavigationBar;
- (void)constructUI;

@end


@implementation MenuTree

@synthesize menuTreeRequested;

- (MenuTree *)init
{
    if (self = [super init])
    {
        menuTreeRequested = NO;
        
        _childrenList = [[NSMutableArray alloc] init];
        _ancestorList = [[NSMutableArray alloc] init];
        
        _navigationView = [[UIView alloc] init];
        _navigationBar = [[UIView alloc] init];
        _backButton = [[UIButton alloc] init];
        _menuTable = [[MenuTable alloc] init];
        _orderedList = [[MenuListView alloc] init];
        _curveView = [[CurveView alloc] init];
        _textInputView = [[TextInputView alloc] init];
        _multiActionView = [[MultiActionView alloc] init];
        
        [_backButton addTarget:self action:@selector(navigateBack) forControlEvents:UIControlEventTouchUpInside];
        
        [_menuTable setDataSource:self];
        [_menuTable setDelegate:self];
        [_orderedList setDelegate:self];
        [_curveView setDelegate:self];

        [self constructUI];
    }
    
    return self;
}

- (BOOL)isVisible
{
    return self.superview != nil;
}

- (void)setMenuData:(const rcp_cur_menu_cb_data_t *)data
{
    [_childrenList removeAllObjects];
    [_ancestorList removeAllObjects];
    
    if (_orderedList.superview != nil)
    {
        [_orderedList removeFromSuperview];
    }
    
    if (_curveView.superview != nil)
    {
        [_curveView removeFromSuperview];
    }
    
    rcp_menu_node_list_t *cur = data->children_list;
    while (cur)
    {
        if (cur->info != NULL)
        {
            MenuNode *node = [[MenuNode alloc] initWithInfo:cur->info];
            [_childrenList addObject:node];
        }
        
        cur = cur->next;
    }
    
    cur = data->ancestor_list;
    while (cur)
    {
        if (cur->info != NULL)
        {
            MenuNode *node = [[MenuNode alloc] initWithInfo:cur->info];
            [_ancestorList addObject:node];
        }
        
        cur = cur->next;
    }
    
    const NSUInteger count = [_childrenList count];
    BOOL maxRowCountExceeded = (count > MAX_ROW_COUNT);
    [_menuTable setFrame:CGRectMake(_menuTable.frame.origin.x, _menuTable.frame.origin.y, _menuTable.frame.size.width, 44.0 * (maxRowCountExceeded ? MAX_ROW_COUNT : count))];
    [_menuTable setScrollEnabled:maxRowCountExceeded];
    [_backButton setTitle:(([_ancestorList count] > 1) ? @"Back" : @"Close") forState:UIControlStateNormal];
    [self constructNavigationBar];
    [_menuTable reloadData];
}

- (void)show
{
    if (self.superview == nil)
    {
        TabViewController *tabVC = [TabViewController instance];
        [tabVC.view addSubview:self];
    }
}

- (void)hide
{
    if (self.superview != nil)
    {
        [self removeFromSuperview];
    }
}

- (void)updateParam:(rcp_param_t)paramID integer:(const int)val infoIsValid:(BOOL)infoIsValid info:(rcp_cur_int_edit_info_t)editInfo
{
    int i;
    const NSUInteger count = [_childrenList count];
    
    for (i = 0; i < count; i++)
    {
        @try {
            MenuTableViewCell *cell = (MenuTableViewCell *)[_menuTable cellForRowAtIndexPath:[NSIndexPath indexPathForRow:i inSection:0]];
            
            if (cell.paramID == paramID)
            {
                [cell updateInt:val infoIsValid:infoIsValid info:editInfo];
            }
        }
        @catch (NSException *exception) {
            
        }
    }
}

- (void)updateParam:(rcp_param_t)paramID uinteger:(const uint32_t)val infoIsValid:(BOOL)infoIsValid info:(rcp_cur_uint_edit_info_t)editInfo
{
    int i;
    const NSUInteger count = [_childrenList count];
    
    for (i = 0; i < count; i++)
    {
        @try {
            MenuTableViewCell *cell = (MenuTableViewCell *)[_menuTable cellForRowAtIndexPath:[NSIndexPath indexPathForRow:i inSection:0]];
            
            if (cell.paramID == paramID)
            {
                [cell updateUInt:val infoIsValid:infoIsValid info:editInfo];
            }
        }
        @catch (NSException *exception) {
            
        }
    }
}

- (void)updateParam:(rcp_param_t)paramID string:(const char *)str status:(rcp_param_status_t)status
{
    int i;
    const NSUInteger count = [_childrenList count];
    
    for (i = 0; i < count; i++)
    {
        @try {
            MenuTableViewCell *cell = (MenuTableViewCell *)[_menuTable cellForRowAtIndexPath:[NSIndexPath indexPathForRow:i inSection:0]];
            
            if (cell.paramID == paramID)
            {
                if (cell.curveViewRequested)
                {
                    cell.curveViewRequested = NO;
                    
                    if (_curveView.superview == nil)
                    {
                        [_curveView setParam:cell.paramID];
                        [_curveView setCurve:str status:status param:cell.paramID];
                        [self addSubview:_curveView];
                    }
                }
                else if (_curveView.superview != nil && cell.paramID == _curveView.paramID)
                {
                    [_curveView setCurve:str status:status param:cell.paramID];
                }
                else
                {
                    [cell updateStr:str status:status];
                }
            }
        }
        @catch (NSException *exception) {
            
        }
    }
}

- (void)updateParam:(rcp_param_t)paramID string:(const char *)str status:(rcp_param_status_t)status infoIsValid:(BOOL)infoIsValid info:(rcp_cur_str_edit_info_t)editInfo
{
    int i;
    const NSUInteger count = [_childrenList count];
    
    for (i = 0; i < count; i++)
    {
        @try {
            MenuTableViewCell *cell = (MenuTableViewCell *)[_menuTable cellForRowAtIndexPath:[NSIndexPath indexPathForRow:i inSection:0]];
            
            if (cell.paramID == paramID)
            {
                if (cell.curveViewRequested)
                {
                    cell.curveViewRequested = NO;
                    
                    if (_curveView.superview == nil)
                    {
                        [_curveView setParam:cell.paramID];
                        [_curveView setCurve:str status:status param:cell.paramID];
                        [self addSubview:_curveView];
                    }
                }
                else if (_curveView.superview != nil && cell.paramID == _curveView.paramID)
                {
                    [_curveView setCurve:str status:status param:cell.paramID];
                }
                else
                {
                    [cell updateStr:str status:status infoIsValid:infoIsValid info:editInfo];
                }
            }
        }
        @catch (NSException *exception) {
            
        }
    }
}

- (void)updateParam:(rcp_param_t)paramID list:(RCPList *)list
{
    int i;
    const NSUInteger count = [_childrenList count];
    
    for (i = 0; i < count; i++)
    {
        @try {
            MenuTableViewCell *cell = (MenuTableViewCell *)[_menuTable cellForRowAtIndexPath:[NSIndexPath indexPathForRow:i inSection:0]];
            
            if (cell.paramID == paramID)
            {
                if (cell.orderedListViewRequested)
                {
                    cell.orderedListViewRequested = NO;
                    
                    if (_orderedList.superview == nil)
                    {
                        _orderedList.rcpList = list;
                        [self addSubview:_orderedList];
                    }
                }
                else
                {
                    [cell updateList:list];
                }
            }
        }
        @catch (NSException *exception) {
            
        }
    }
    
    if (_multiActionView.paramID == list.paramID)
    {
        [_multiActionView setTableList:list];
    }
}

- (void)updateParam:(rcp_param_t)paramID enabled:(BOOL)enabled
{
    int i;
    const NSUInteger count = [_childrenList count];
    
    for (i = 0; i < count; i++)
    {
        @try {
            MenuTableViewCell *cell = (MenuTableViewCell *)[_menuTable cellForRowAtIndexPath:[NSIndexPath indexPathForRow:i inSection:0]];
            
            if (cell.paramID == paramID)
            {
                [cell updateStatus:enabled];
            }
        }
        @catch (NSException *exception) {
            
        }
    }
    
    if (_multiActionView.superview != nil)
    {
        [_multiActionView setEnabledStatus:enabled ofButtonWithParamID:paramID];
    }
}

- (void)updateNode:(rcp_menu_node_id_t)nodeID enabled:(BOOL)enabled
{
    int i;
    const NSUInteger count = [_childrenList count];
    
    for (i = 0; i < count; i++)
    {
        @try {
            MenuTableViewCell *cell = (MenuTableViewCell *)[_menuTable cellForRowAtIndexPath:[NSIndexPath indexPathForRow:i inSection:0]];
            
            if (cell.nodeID == nodeID)
            {
                [cell updateStatus:enabled];
            }
        }
        @catch (NSException *exception) {
            
        }
    }
}

- (void)keyboardWillShow:(CGRect)keyboardFrame
{
    int i;
    const NSUInteger count = [_childrenList count];
    
    for (i = 0; i < count; i++)
    {
        @try {
            MenuTableViewCell *cell = (MenuTableViewCell *)[_menuTable cellForRowAtIndexPath:[NSIndexPath indexPathForRow:i inSection:0]];
            
            if ([cell hasOpenPicker])
            {
                [cell keyboardWillShow];
                
                const CGFloat cellBottom = _menuTable.frame.origin.y + cell.frame.origin.y + cell.frame.size.height - _menuTable.contentOffset.y;
                const CGFloat keyboardTop = keyboardFrame.origin.y - keyboardFrame.size.height;

                if (cellBottom > keyboardTop)
                {
                    CGFloat offset = cellBottom - keyboardTop;
                    
                    [UIView beginAnimations:nil context:NULL];
                    [UIView setAnimationDuration:0.3];
                    
                    CGRect viewFrame = self.frame;
                    viewFrame.size.height += keyboardFrame.size.height;
                    viewFrame.origin.y -= offset;
                    self.frame = viewFrame;
                    
                    [UIView commitAnimations];
                }
                
                break;
            }
        }
        @catch (NSException *exception) {
            
        }
    }
}

- (void)keyboardWillHide:(CGRect)keyboardFrame
{
    int i;
    const NSUInteger count = [_childrenList count];
    
    for (i = 0; i < count; i++)
    {
        @try {
            MenuTableViewCell *cell = (MenuTableViewCell *)[_menuTable cellForRowAtIndexPath:[NSIndexPath indexPathForRow:i inSection:0]];
            
            if ([cell hasOpenPicker])
            {
                [cell keyboardWillHide];
                
                CGRect viewFrame = self.frame;
                
                // Ensures that the view has actually been moved up
                if (viewFrame.origin.y < 0.0)
                {
                    [UIView beginAnimations:nil context:NULL];
                    [UIView setAnimationDuration:0.3];
                    
                    viewFrame.size.height -= keyboardFrame.size.height;
                    viewFrame.origin.y = 0.0;
                    self.frame = viewFrame;
                    
                    [UIView commitAnimations];
                }
                
                break;
            }
        }
        @catch (NSException *exception) {
            
        }
    }
}

- (void)navigateBack
{
    const NSUInteger count = [_ancestorList count];
    
    if (count > 1)
    {
        menuTreeRequested = YES;
        
        RCPHandler *handler = [RCPHandler instance];
        MenuNode *node = [_ancestorList objectAtIndex:0]; // the top ancestor is one level up
        
       [handler rcpGetMenu:node.parentID];
    }
    else
    {
        [self hide];
    }
}

- (void)navigationButtonPressed:(UIButton *)sender
{
    const NSInteger index = sender.tag;
    
    if (index < [_ancestorList count])
    {
        menuTreeRequested = YES;
        
        RCPHandler *handler = [RCPHandler instance];
        MenuNode *node = [_ancestorList objectAtIndex:index];
        
        [handler rcpGetMenu:node.nodeID];
    }
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView
{
    return 1;
}

- (NSInteger)tableView:(UITableView*)tableView numberOfRowsInSection:(NSInteger)section
{
    return [_childrenList count];
}

- (UITableViewCell *)tableView:(UITableView*)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
    static NSString *cellIdentifier = @"MenuNode";
    MenuTableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:cellIdentifier];
    MenuNode *node = [_childrenList objectAtIndex:indexPath.row];
    
    if (cell == nil)
    {
        cell = [[MenuTableViewCell alloc] initWithReuseIdentifier:cellIdentifier];
        
        [cell setBackgroundColor:_menuTable.backgroundColor];
        [cell.contentView setBackgroundColor:_menuTable.backgroundColor];
        [cell setTintColor:[UIColor whiteColor]];
        [cell setDelegate:self];
    }
    
    cell.hasFocus = NO;
    
    if (node.type == RCP_MENU_NODE_TYPE_BRANCH)
    {
        // Initially enables the cell since the status callback won't have any effect on a branch since it doesn't have a valid rcp_param_t
        [cell updateStatus:YES];
    }
    
    [cell setNode:node];
    
    return cell;
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath
{
    [tableView deselectRowAtIndexPath:indexPath animated:YES];
    
    if (indexPath.row < [_childrenList count])
    {
        RCPHandler *handler = [RCPHandler instance];
        MenuNode *node = [_childrenList objectAtIndex:indexPath.row];
        MenuTableViewCell *cell = (MenuTableViewCell *)[_menuTable cellForRowAtIndexPath:indexPath];
        
        if (node.type == RCP_MENU_NODE_TYPE_BRANCH)
        {
            menuTreeRequested = YES;
            [handler rcpGetMenu:node.nodeID];
        }
        else if (node.type == RCP_MENU_NODE_TYPE_CURVE_LEAF)
        {
            cell.curveViewRequested = YES;
            [handler rcpGet:node.paramID];
        }
        else if (node.type == RCP_MENU_NODE_TYPE_IP_ADDRESS_LEAF)
        {
            if (_textInputView.superview == nil)
            {
                _textInputView.paramID = node.paramID;
                _textInputView.textInputViewType = TEXT_INPUT_VIEW_TYPE_IP_ADDRESS;
                _textInputView.textColor = [UIColor whiteColor];
                [_textInputView setIPAddress:cell.ipAddress];
                
                [_textInputView showInView:self];
            }
        }
        else if (node.type == RCP_MENU_NODE_TYPE_LIST_LEAF)
        {
            [cell requestPicker];
        }
        else if (node.type == RCP_MENU_NODE_TYPE_NUMBER_LEAF)
        {
            [cell requestTextInputView];
        }
        else if (node.type == RCP_MENU_NODE_TYPE_TEXT_LEAF)
        {
            [cell requestTextInputView];
        }
        else if (node.type == RCP_MENU_NODE_TYPE_ORDERED_LIST_LEAF)
        {
            cell.orderedListViewRequested = YES;
            [handler rcpGetList:node.paramID];
        }
        else if (node.type == RCP_MENU_NODE_TYPE_TIMECODE_LEAF)
        {
            _textInputView.paramID = node.paramID;
            _textInputView.textInputViewType = TEXT_INPUT_VIEW_TYPE_TIMECODE;
            _textInputView.textColor = [UIColor whiteColor];
            [_textInputView setTimecode:cell.timecode];
            
            [_textInputView showInView:self];
        }
        else if (node.type == RCP_MENU_NODE_TYPE_MULTI_ACTION_LIST_LEAF)
        {
            [_multiActionView setParamID:node.paramID];
            [_multiActionView requestToOpen];
            [_multiActionView removeAllButtons];
            
            for (RCPListItem *item in node.args)
            {
                [_multiActionView addButtonWithTitle:item.str value:item.num];
            }
        }
        else if (node.type == RCP_MENU_NODE_TYPE_NOT_YET_SUPPORTED_LEAF)
        {
            [RCPUtil showDialogWithTitle:@"Not Supported on iOS" message:@"Usage of this menu item is currently limited to the camera."];
        }
    }
}

- (NSIndexPath *)tableView:(UITableView *)tableView willSelectRowAtIndexPath:(NSIndexPath *)indexPath
{
    if (indexPath.row < [_childrenList count])
    {
        MenuNode *node = [_childrenList objectAtIndex:indexPath.row];
        
        if (node.type == RCP_MENU_NODE_TYPE_ACTION_LEAF ||
            node.type == RCP_MENU_NODE_TYPE_ENABLE_LEAF ||
            node.type == RCP_MENU_NODE_TYPE_DATETIME_LEAF ||
            node.type == RCP_MENU_NODE_TYPE_STATUS_LEAF ||
            (node.type == RCP_MENU_NODE_TYPE_LIST_LEAF && [RCPUtil menuParamHasSlider:node.paramID]))
        {
            return nil;
        }
    }

    return indexPath;
}

- (void)menuTableViewCellSelected:(MenuTableViewCell *)cell
{
    [_menuTable setSelectedCell:cell];
}

- (void)menuTableViewCell:(MenuTableViewCell *)cell textInputViewRequestedFromPickerControl:(PickerControl *)pickerControl
{
    if (_textInputView.superview == nil)
    {
        pickerControl.ignoreCameraListUpdates = NO;
        
        _textInputView.paramID = pickerControl.paramID;
        _textInputView.textInputViewType = TEXT_INPUT_VIEW_TYPE_TEXTFIELD;
        _textInputView.sendValueType = pickerControl.rcpList.sendValueType;
        _textInputView.textColor = pickerControl.textFieldTextColor;
        [_textInputView setText:pickerControl.baseValue];
        [_textInputView setDelegate:pickerControl];
        
        switch (pickerControl.rcpList.sendValueType)
        {
            case SEND_VALUE_TYPE_INT:
                [_textInputView setIntEditInfo:pickerControl.intEditInfo];
                break;
                
            case SEND_VALUE_TYPE_UINT:
                [_textInputView setUintEditInfo:pickerControl.uintEditInfo];
                break;
                
            case SEND_VALUE_TYPE_STR:
                [_textInputView setStrEditInfo:pickerControl.strEditInfo];
                break;
                
            default:
                break;
        }
        
        [_textInputView showInView:self];
    }
}

- (void)menuTableViewCell:(MenuTableViewCell *)cell textInputViewRequestedFromTextControl:(TextControl *)textControl
{
    if (_textInputView.superview == nil)
    {
        _textInputView.paramID = textControl.paramID;
        _textInputView.textInputViewType = TEXT_INPUT_VIEW_TYPE_TEXTFIELD;
        _textInputView.sendValueType = textControl.sendValueType;
        _textInputView.textColor = textControl.valueTextColor;
        [_textInputView setText:textControl.baseValue];
        [_textInputView setDelegate:textControl];
        
        switch (textControl.sendValueType)
        {
            case SEND_VALUE_TYPE_INT:
                _textInputView.intEditInfo = textControl.intEditInfo;
                break;
                
            case SEND_VALUE_TYPE_UINT:
                _textInputView.uintEditInfo = textControl.uintEditInfo;
                break;
                
            case SEND_VALUE_TYPE_STR:
                _textInputView.strEditInfo = textControl.strEditInfo;
                break;
                
            default:
                break;
        }
        
        [_textInputView showInView:self];
    }
}

- (void)menuListViewFinishedEditing:(MenuListView *)menuListView
{
    if (_orderedList.superview != nil)
    {
        [_orderedList removeFromSuperview];
    }
}

- (void)curveViewFinishedEditing:(CurveView *)curveView
{
    if (_curveView.superview != nil)
    {
        [_curveView removeFromSuperview];
    }
}

- (void)multiActionViewReadyToOpen:(MultiActionView *)multiActionView
{
    [multiActionView showInView:self];
}

- (void)constructNavigationBar
{
    UIFont *font = _backButton.titleLabel.font;
    UIColor *textColor = [UIColor whiteColor];
    
    CGSize textSize;
    float x;
    float spacerWidth;
    float navWidth = 0.0;
    
    NSUInteger i;
    const NSUInteger count = [_ancestorList count];
    int *buttonWidths = (int *)calloc(count, sizeof(int));
    MenuNode *node = nil;
    
    
    // Remove all items from the navigation bar
    for (UIView *view in _navigationBar.subviews)
    {
        [view removeFromSuperview];
    }
    
    
    // Calculate how wide the navigation path is
    for (i = 0; i < count; i++)
    {
        node = [_ancestorList objectAtIndex:i];
        textSize = [RCPUtil sizeOfText:node.title font:font];
        
        buttonWidths[i] = textSize.width + 1;
        navWidth += textSize.width + 1;
    }
    
    spacerWidth = [RCPUtil sizeOfText:@" > " font:font].width;
    navWidth += (count - 1) * spacerWidth;
    
    
    if (navWidth > _navigationBar.bounds.size.width)
    {
        x = _navigationBar.bounds.size.width - navWidth;
    }
    else
    {
        x = 0.0;
    }
    
    // Add buttons and spacers to navigation bar, excluding the last entry in the path
    for (i = count - 1; i > 0; i--)
    {
        node = [_ancestorList objectAtIndex:i];
        UIButton *button = [[UIButton alloc] init];
        UILabel *spacerLabel = [[UILabel alloc] init];
        
        [button setFrame:CGRectMake(x, 0.0, buttonWidths[i], _navigationBar.bounds.size.height)];
        [spacerLabel setFrame:CGRectMake(x + button.frame.size.width, 0.0, spacerWidth, _navigationBar.bounds.size.height)];
        
        [button.titleLabel setFont:font];
        [button setTitleColor:textColor forState:UIControlStateNormal];
        [button setTitleColor:[UIColor grayColor] forState:UIControlStateHighlighted];
        [button setTitle:node.title forState:UIControlStateNormal];
        [button setTag:i];
        [button addTarget:self action:@selector(navigationButtonPressed:) forControlEvents:UIControlEventTouchUpInside];
        
        [spacerLabel setFont:font];
        [spacerLabel setText:@">"];
        [spacerLabel setTextAlignment:NSTextAlignmentCenter];
        [spacerLabel setTextColor:textColor];
        
        
        x += button.frame.size.width + spacerLabel.frame.size.width;
        
        [_navigationBar addSubview:button];
        [_navigationBar addSubview:spacerLabel];
    }
    
    
    // Add the last entry, which is a label instead of a button
    node = [_ancestorList objectAtIndex:0];
    UILabel *nodeLabel = [[UILabel alloc] init];
    
    [nodeLabel setFont:font];
    [nodeLabel setText:node.title];
    [nodeLabel setTextAlignment:NSTextAlignmentCenter];
    [nodeLabel setTextColor:[UIColor whiteColor]];
    
    [nodeLabel setFrame:CGRectMake(x, 0.0, buttonWidths[0], _navigationBar.bounds.size.height)];
    
    [_navigationBar addSubview:nodeLabel];
    
    free(buttonWidths);
}

- (void)constructUI
{
    UIColor *backgroundColor = [UIColor blackColor];
    UIColor *navViewColor = [UIColor colorWithRed:(85.0 / 255.0) green:(85.0 / 255.0) blue:(85.0 / 255.0) alpha:1.0];
    UIColor *navbarColor = [UIColor clearColor];
    UIColor *tableColor = [UIColor blackColor];
    
    const int rowHeight = 44.0;
    
    const float topOffset = 20.0;
    
    const float navbarSpacing = 10.0;
    
    const float navViewWidth = VIEW_WIDTH;
    const float navViewHeight = rowHeight + topOffset;
    
    const float tableWidth = navViewWidth;
    const float tableHeight = rowHeight * MAX_ROW_COUNT;
    
    NSString *buttonText = @"Close";
    UIFont *buttonFont = [UIFont systemFontOfSize:16.0];
    const CGSize buttonSize = CGSizeMake([RCPUtil sizeOfText:buttonText font:buttonFont].width, rowHeight);
    const float buttonSpacing = 10.0;
    
    
    [self setFrame:CGRectMake(0.0, 0.0, VIEW_WIDTH, VIEW_HEIGHT)];
    [_navigationView setFrame:CGRectMake(0.0, 0.0, navViewWidth, navViewHeight)];
    [_backButton setFrame:CGRectMake(navViewWidth - buttonSize.width - buttonSpacing, navViewHeight - buttonSize.height, buttonSize.width, buttonSize.height)];
    [_navigationBar setFrame:CGRectMake(navbarSpacing, _backButton.frame.origin.y, _backButton.frame.origin.x - (navbarSpacing * 2.0), _backButton.bounds.size.height)];
    [_menuTable setFrame:CGRectMake(0.0, _navigationView.frame.origin.y + _navigationView.frame.size.height, tableWidth, tableHeight)];
    [_orderedList setFrame:CGRectMake(_menuTable.frame.origin.x, _menuTable.frame.origin.y, VIEW_WIDTH, VIEW_HEIGHT - _menuTable.frame.origin.y)];
    [_curveView setFrame:CGRectMake(_menuTable.frame.origin.x, _menuTable.frame.origin.y, VIEW_WIDTH, VIEW_HEIGHT - _menuTable.frame.origin.y)];
    [_textInputView setFrame:CGRectMake(_menuTable.frame.origin.x, _menuTable.frame.origin.y, VIEW_WIDTH, VIEW_HEIGHT - _menuTable.frame.origin.y)];
    
    [self setBackgroundColor:backgroundColor];
    [_navigationView setBackgroundColor:navViewColor];
    [_navigationBar setBackgroundColor:navbarColor];
    [_menuTable setBackgroundColor:tableColor];
    
    [_navigationBar setClipsToBounds:YES];
    
    [_backButton.titleLabel setFont:buttonFont];
    [_backButton setTitleColor:[UIColor colorWithRed:0.0 green:0.0 blue:0.0 alpha:1.0] forState:UIControlStateNormal];
    [_backButton setTitleColor:[UIColor colorWithRed:0.0 green:0.0 blue:0.0 alpha:0.5] forState:UIControlStateHighlighted];
    
    if ([_menuTable respondsToSelector:@selector(setSeparatorStyle:)])
    {
        [_menuTable setSeparatorStyle:UITableViewCellSeparatorStyleNone];
    }
    
    [_curveView showCurves:YES];
    [_curveView setEnabled:YES];
    
    [_multiActionView setDelegate:self];
    
    [_navigationView addSubview:_navigationBar];
    [_navigationView addSubview:_backButton];
    
    [self addSubview:_navigationView];
    [self addSubview:_menuTable];
}

@end
