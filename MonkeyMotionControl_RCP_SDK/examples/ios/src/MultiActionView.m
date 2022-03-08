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

#import "MultiActionView.h"
#import "RCPHandler.h"
#import "RCPUtil.h"
#import "Logger.h"

@interface MultiActionView()

@property (assign, nonatomic) BOOL listRequested;
@property (retain, nonatomic) RCPList *tableList;
@property (retain, nonatomic) NSMutableArray *buttons;
@property (assign, nonatomic) NSInteger selectedRow;

@property (retain, nonatomic) UIView *container;
@property (retain, nonatomic) UILabel *title;
@property (retain, nonatomic) UIView *titleSeparator;
@property (retain, nonatomic) UITableView *table;
@property (retain, nonatomic) UILabel *emptyLabel;
@property (retain, nonatomic) UIView *tableSeparator;
@property (retain, nonatomic) UIView *doneButtonSeparator;
@property (retain, nonatomic) UIButton *doneButton;


- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView;
- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section;
- (UITableViewCell *)tableView:(UITableView*)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath;
- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath;

- (BOOL)selectRow:(NSInteger)row;

- (void)buttonPressed:(UIButton *)sender;
- (void)donePressed;

- (void)initialize;

@end


@implementation MultiActionView

@synthesize delegate, paramID;

- (id)init
{
    if (self = [super init])
    {
        [self initialize];
    }
    
    return self;
}

- (id)initWithCoder:(NSCoder *)aDecoder
{
    if (self = [super initWithCoder:aDecoder])
    {
        [self initialize];
    }
    
    return self;
}

- (id)initWithFrame:(CGRect)frame
{
    if (self = [super initWithFrame:frame])
    {
        [self initialize];
    }
    
    return self;
}

- (BOOL)listRequested
{
    return _listRequested;
}

- (void)setFrame:(CGRect)frame
{
    const int numOfButtonsPerRow = 2;
    
    const CGFloat verticalSpacing = 7.0;
    const CGFloat horizontalSpacing = 7.0;
    const CGFloat horizontalPadding = 7.0;
    const CGFloat containerWidth = frame.size.width * 0.85;
    
    const CGSize titleSize = CGSizeMake(containerWidth, 45.0);
    const CGSize doneButtonSize = titleSize;
    const CGSize separatorSize = CGSizeMake(containerWidth, 0.5);
    
    const int buttonRowCount = ceil((double)[_buttons count] / (double)numOfButtonsPerRow);
    const CGFloat buttonWidth = (containerWidth - (horizontalPadding * 2.0) - ((numOfButtonsPerRow - 1) * horizontalSpacing)) / (double)numOfButtonsPerRow;
    const CGFloat buttonHeight = 40.0;
    const CGFloat buttonViewHeight = (buttonRowCount * buttonHeight) + ((buttonRowCount - 1) * verticalSpacing);
    int buttonIndex = 0;
    int buttonRow = 0;
    CGFloat buttonX;
    
    const CGFloat containerMaxHeight = frame.size.height * 0.85;
    const CGFloat desiredTableHeight = [_tableList count] * [_table rowHeight];
    const CGFloat desiredContainerHeight = titleSize.height + separatorSize.height + desiredTableHeight + verticalSpacing + buttonViewHeight + verticalSpacing + separatorSize.height + doneButtonSize.height;
    const BOOL containerMaxHeightExceeded = desiredContainerHeight > containerMaxHeight;
    CGFloat tableHeight;
    
    if (containerMaxHeightExceeded)
    {
        tableHeight = containerMaxHeight - (titleSize.height + separatorSize.height + verticalSpacing + buttonViewHeight + verticalSpacing + separatorSize.height + doneButtonSize.height);
        [_table setScrollEnabled:YES];
        [_tableSeparator setHidden:NO];
    }
    else
    {
        tableHeight = desiredTableHeight;
        [_table setScrollEnabled:NO];
        [_tableSeparator setHidden:YES];
    }
    
    const CGSize tableSize = CGSizeMake(containerWidth, tableHeight);
    const CGSize emptyLabelSize = CGSizeMake(tableSize.width, [_table rowHeight] * 2.0);
    
    CGFloat y = 0.0;
    
    
    const CGFloat maxTitleFontSize = 18.0;
    const CGFloat biggestPossibleTitleFontSize = [RCPUtil maxSystemFontSizeForContainer:CGRectMake(0.0, 0.0, titleSize.width, titleSize.height) bold:YES];
    
    const CGFloat maxEmptyLabelFontSize = 16.0;
    const CGFloat minEmptyLabelFontSize = 8.0;
    CGFloat emptyLabelFontSize = [RCPUtil maxSystemFontSizeForContainer:CGRectMake(0.0, 0.0, emptyLabelSize.width, emptyLabelSize.height) bold:YES];
    
    if (emptyLabelFontSize > maxEmptyLabelFontSize)
    {
        emptyLabelFontSize = maxEmptyLabelFontSize;
    }
    else if (emptyLabelFontSize < minEmptyLabelFontSize)
    {
        emptyLabelFontSize = minEmptyLabelFontSize;
    }
    
    [_title setFont:[UIFont boldSystemFontOfSize:((biggestPossibleTitleFontSize > maxTitleFontSize) ? maxTitleFontSize : biggestPossibleTitleFontSize)]];
    [_emptyLabel setFont:[UIFont boldSystemFontOfSize:emptyLabelFontSize]];
    
    
    [super setFrame:frame];
    
    [_title setFrame:CGRectMake(0.0, y, titleSize.width, titleSize.height)];
    y += titleSize.height;
    
    [_titleSeparator setFrame:CGRectMake(0.0, y, separatorSize.width, separatorSize.height)];
    y += separatorSize.height;
    
    [_table setFrame:CGRectMake(0.0, y, tableSize.width, tableSize.height)];
    [_emptyLabel setFrame:CGRectMake(0.0, y, emptyLabelSize.width, emptyLabelSize.height)];
    y += (tableSize.height == 0) ? emptyLabelSize.height : tableSize.height;
    
    if (!_tableSeparator.hidden)
    {
        [_tableSeparator setFrame:CGRectMake(0.0, _table.frame.origin.y + tableSize.height, separatorSize.width, separatorSize.height)];
        y += separatorSize.height;
    }
    
    buttonX = horizontalPadding;
    y += verticalSpacing - (buttonHeight + verticalSpacing);
    
    for (UIButton *button in _buttons)
    {
        // If a new row (including the first row) is encountered
        if ((buttonIndex % numOfButtonsPerRow) == 0)
        {
            buttonRow++;
            
            // If this is the last row
            if (buttonRow == buttonRowCount)
            {
                int buttonsRemaining = [_buttons count] - buttonIndex;
                buttonX = (containerWidth - ((buttonsRemaining * buttonWidth) + ((buttonsRemaining - 1) * horizontalSpacing))) / 2.0;
            }
            else
            {
                buttonX = horizontalPadding;
            }
            
            y += buttonHeight + verticalSpacing;
        }
        else
        {
            buttonX += buttonWidth + horizontalSpacing;
        }
        
        [button setFrame:CGRectMake(buttonX, y, buttonWidth, buttonHeight)];
        
        buttonIndex++;
    }
    
    y += buttonHeight + verticalSpacing;
    
    [_doneButtonSeparator setFrame:CGRectMake(0.0, y, separatorSize.width, separatorSize.height)];
    y += separatorSize.height;
    
    [_doneButton setFrame:CGRectMake(0.0, y, doneButtonSize.width, doneButtonSize.height)];
    y += doneButtonSize.height;
    
    [_container setFrame:CGRectMake((frame.size.width - containerWidth) / 2.0, (frame.size.height - y) / 2.0, containerWidth, y)];
}

- (void)setParamID:(rcp_param_t)param
{
    paramID = param;
    
    RCPHandler *handler = [RCPHandler instance];
    [_title setText:[handler rcpGetLabel:paramID]];
}

- (void)setTableList:(RCPList *)list
{
    _tableList = list;
    BOOL listIsEmpty = ([_tableList count] == 0);
    
    [_emptyLabel setHidden:!listIsEmpty];
    [_table setHidden:listIsEmpty];
    
    if (_listRequested)
    {
        _listRequested = NO;
        
        if ([self selectRow:0])
        {
            [_table scrollToRowAtIndexPath:[NSIndexPath indexPathForRow:_selectedRow inSection:0] atScrollPosition:UITableViewScrollPositionTop animated:NO];
        }
        
        if (delegate != nil)
        {
            [delegate multiActionViewReadyToOpen:self];
        }
    }
    else
    {
        const NSUInteger count = [_tableList count];
        
        if (_selectedRow < count)
        {
            [self selectRow:_selectedRow];
        }
        else
        {
            [self selectRow:count - 1];
        }
        
        [_table reloadData];
    }
}

- (void)addButtonWithTitle:(NSString *)title value:(const int32_t)value
{
    UIButton *button = [[UIButton alloc] init];
    UIColor *buttonTitleColorNormal = [UIColor whiteColor];
    UIColor *buttonTitleColorHighlighted = [UIColor colorWithWhite:1.0 alpha:0.5];
    
    [button setTitle:title forState:UIControlStateNormal];
    [button setTag:value];
    
    [button setBackgroundColor:[UIColor clearColor]];
    [button.titleLabel setFont:[UIFont boldSystemFontOfSize:18.0]];
    [button setTitleColor:buttonTitleColorNormal forState:UIControlStateNormal];
    [button setTitleColor:buttonTitleColorHighlighted forState:UIControlStateHighlighted];
    [button setTitleColor:buttonTitleColorHighlighted forState:UIControlStateDisabled];
    [button.layer setBorderColor:buttonTitleColorNormal.CGColor];
    [button.layer setBorderWidth:1.0];
    [button.layer setCornerRadius:5.0];
    [button addTarget:self action:@selector(buttonPressed:) forControlEvents:UIControlEventTouchUpInside];
    
    [_buttons addObject:button];
    [_container addSubview:button];
}

- (void)removeAllButtons
{
    for (UIButton *button in _buttons)
    {
        if (button.superview != nil)
        {
            [button removeFromSuperview];
        }
    }
    
    [_buttons removeAllObjects];
}

- (void)setEnabledStatus:(BOOL)enabled ofButtonWithParamID:(rcp_param_t)param
{
    for (UIButton *button in _buttons)
    {
        rcp_param_t buttonParamID = (rcp_param_t)button.tag;
        
        if (buttonParamID == param)
        {
            [button setEnabled:button.enabled && enabled];
        }
    }
}

- (void)requestToOpen
{
    RCPHandler *handler = [RCPHandler instance];
    
    _listRequested = YES;
    [handler rcpGetList:paramID];
}

- (void)showInView:(UIView *)view
{
    [self setFrame:view.bounds];
    
    _container.alpha = 0.0;
    _container.transform = CGAffineTransformMakeScale(1.2, 1.2);
    [view addSubview:self];
    
    [UIView animateWithDuration:0.3 delay:0 options:UIViewAnimationOptionCurveEaseOut animations:^
     {
         _container.alpha = 1.0;
         _container.transform = CGAffineTransformIdentity;
     }
                     completion:^(BOOL finished)
     {
         
     }];
}

- (void)hide
{
    if (self.superview != nil)
    {
        const CGFloat prevAlpha = self.alpha;
        
        [UIView animateWithDuration:0.3 delay:0 options:UIViewAnimationOptionCurveEaseOut animations:^
         {
             _container.alpha = 0.0;
             _container.transform = CGAffineTransformMakeScale(0.9, 0.9);
             self.alpha = 0.0;
         }
                         completion:^(BOOL finished)
         {
             [self removeFromSuperview];
             
             self.alpha = prevAlpha;
             _container.alpha = 1.0;
             _container.transform = CGAffineTransformIdentity;
         }];
    }
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView
{
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section
{
    return [_tableList count];
}

- (UITableViewCell *)tableView:(UITableView*)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
    static NSString *cellIdentifier = @"TableEntry";
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:cellIdentifier];
    RCPListItem *item = [_tableList itemAtIndex:indexPath.row];
    
    if (cell == nil)
    {
        cell = [[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:cellIdentifier];
        [cell setBackgroundColor:_table.backgroundColor];
        [cell.textLabel setTextColor:[UIColor whiteColor]];
        [cell setTintColor:[UIColor whiteColor]];
        [cell.textLabel setFont:[UIFont systemFontOfSize:14.0]];
        
        if ([cell respondsToSelector:@selector(setLayoutMargins:)])
        {
            [cell setLayoutMargins:UIEdgeInsetsZero];
        }
        
        if ([cell respondsToSelector:@selector(setSeparatorInset:)])
        {
            [cell setSeparatorInset:UIEdgeInsetsZero];
        }
    }
    
    if (indexPath.row == _selectedRow)
    {
        [cell setAccessoryType:UITableViewCellAccessoryCheckmark];
    }
    else
    {
        [cell setAccessoryType:UITableViewCellAccessoryNone];
    }
    
    [cell.textLabel setText:item.str];
    
    return cell;
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath
{
    [tableView deselectRowAtIndexPath:indexPath animated:YES];
    [self selectRow:indexPath.row];
}

- (BOOL)selectRow:(NSInteger)row
{
    NSUInteger count = [_tableList count];
    
    if (row >= 0 && row < count)
    {
        RCPHandler *handler = [RCPHandler instance];
        RCPListItem *item = [_tableList itemAtIndex:row];
        int32_t buttonMask = item.num;
        
        for (UIButton *button in _buttons)
        {
            rcp_param_t buttonParamID = (rcp_param_t)button.tag;
            
            [button setEnabled:buttonMask & 1];
            buttonMask = buttonMask >> 1;
            
            [handler rcpGetStatus:buttonParamID];
        }
        
        _selectedRow = row;
        [_table reloadData];
        
        return YES;
    }
    
    return NO;
}

- (void)buttonPressed:(UIButton *)sender
{
    RCPHandler *handler = [RCPHandler instance];
    rcp_param_t buttonParamID = (rcp_param_t)sender.tag;
    rcp_param_properties_t properties = [handler rcpGetProperties:buttonParamID];
    
    if (properties.has_set_str)
    {
        RCPListItem *selectedItem = [_tableList itemAtIndex:_selectedRow];
        [handler rcpSetString:buttonParamID value:selectedItem.str];
    }
    else if (properties.has_send)
    {
        [handler rcpSend:buttonParamID];
    }
}

- (void)donePressed
{
    [self hide];
}

- (void)initialize
{
    delegate = nil;
    paramID = RCP_PARAM_COUNT;
    
    _listRequested = NO;
    _tableList = [[RCPList alloc] init];
    _buttons = [[NSMutableArray alloc] init];
    [self selectRow:0];
    
    _container = [[UIView alloc] init];
    _title = [[UILabel alloc] init];
    _titleSeparator = [[UIView alloc] init];
    _table = [[UITableView alloc] init];
    _emptyLabel = [[UILabel alloc] init];
    _tableSeparator = [[UIView alloc] init];
    _doneButtonSeparator = [[UIView alloc] init];
    _doneButton = [[UIButton alloc] init];
    
    
    [self setBackgroundColor:[UIColor colorWithRed:0.0 green:0.0 blue:0.0 alpha:0.4]];
    
    [_container setBackgroundColor:[UIColor colorWithWhite:0.1 alpha:1.0]];
    [_container.layer setCornerRadius:7.0];
    [_container setClipsToBounds:YES];
    
    [_title setBackgroundColor:[UIColor colorWithWhite:0.2 alpha:1.0]];
    [_title setTextAlignment:NSTextAlignmentCenter];
    [_title setTextColor:[UIColor whiteColor]];
    [_title setNumberOfLines:1];
    
    [_titleSeparator setBackgroundColor:[UIColor colorWithWhite:0.4 alpha:1.0]];
    
    [_table setBackgroundColor:[UIColor clearColor]];
    [_table setRowHeight:44.0];
    [_table setDataSource:self];
    [_table setDelegate:self];
    
    if ([_table respondsToSelector:@selector(setLayoutMargins:)])
    {
        [_table setLayoutMargins:UIEdgeInsetsZero];
    }
    
    if ([_table respondsToSelector:@selector(setSeparatorInset:)])
    {
        [_table setSeparatorInset:UIEdgeInsetsZero];
    }
    
    [_emptyLabel setTextAlignment:NSTextAlignmentCenter];
    [_emptyLabel setTextColor:[UIColor whiteColor]];
    [_emptyLabel setText:@"No Items"];
    
    [_tableSeparator setBackgroundColor:[UIColor colorWithWhite:0.4 alpha:1.0]];
    
    [_doneButtonSeparator setBackgroundColor:[UIColor colorWithWhite:0.4 alpha:1.0]];
    
    [_doneButton setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
    [_doneButton setTitleColor:[UIColor colorWithWhite:1.0 alpha:0.5] forState:UIControlStateHighlighted];
    [_doneButton setBackgroundColor:[UIColor colorWithWhite:0.2 alpha:1.0]];
    [_doneButton.titleLabel setFont:[UIFont boldSystemFontOfSize:18.0]];
    [_doneButton setTitle:@"Done" forState:UIControlStateNormal];
    [_doneButton addTarget:self action:@selector(donePressed) forControlEvents:UIControlEventTouchUpInside];
    
    
    [_container addSubview:_title];
    [_container addSubview:_titleSeparator];
    [_container addSubview:_table];
    [_container addSubview:_emptyLabel];
    [_container addSubview:_tableSeparator];
    [_container addSubview:_doneButtonSeparator];
    [_container addSubview:_doneButton];
    
    [self addSubview:_container];
}

@end
