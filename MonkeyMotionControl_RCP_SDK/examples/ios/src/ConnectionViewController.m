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

#import "ConnectionViewController.h"
#import "RCPHandler.h"
#import "RCPUtil.h"
#import "Logger.h"
#import <net/if.h>
#import <sys/socket.h>
#import <netinet/in.h>
#import <arpa/inet.h>
#import <ifaddrs.h>
#import <netdb.h>

@interface ConnectionViewController ()

@property (weak, nonatomic) IBOutlet UITableView *cameraTable;
@property (weak, nonatomic) IBOutlet UITextField *ipTextField;
@property (weak, nonatomic) IBOutlet UILabel *searchLabel;
@property (weak, nonatomic) IBOutlet UIActivityIndicatorView *searchIndicator;
@property (weak, nonatomic) IBOutlet UIActivityIndicatorView *textFieldIndicator;

@property (retain, nonatomic) NSMutableArray *cameraList; // holds Camera objects
@property (retain, nonatomic) UIActivityIndicatorView *tableIndicator;
@property (assign, nonatomic) int selectedRow;
@property (assign, nonatomic) CGFloat ipTextFieldWidth;


- (void)viewDidLoad;
- (void)didReceiveMemoryWarning;

- (BOOL)textFieldShouldReturn:(UITextField *)textField;

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView;
- (NSInteger)tableView:(UITableView*)tableView numberOfRowsInSection:(NSInteger)section;
- (UITableViewCell*)tableView:(UITableView*)tableView cellForRowAtIndexPath:(NSIndexPath*)indexPath;
- (void)tableView:(UITableView*)tableView didSelectRowAtIndexPath:(NSIndexPath*)indexPath;

- (void)refreshFromPull:(UIRefreshControl *)refreshControl;
- (void)discoverCameras:(id)obj;
- (void)listenForCameras:(id)obj;
void broadcastMessage(const char *data, size_t len, void *user_data);

@end


@implementation ConnectionViewController

@synthesize listeningForCameras, listen;

- (void)setConnected:(BOOL)connected
{
    if (connected)
    {
        RCPHandler *handler = [RCPHandler instance];
        [_ipTextField resignFirstResponder]; // If the user manually entered the IP address, it will get dismissed if the connection is successful
        
        // Attempts to add the connected camera to the list (only applicable if user manually entered the camera's IP address and it wasn't already in the list)
        Camera *camera = [[Camera alloc] initWithCamera:handler.connectedCamera];
        [self addCameraToList:camera];
        
        [_ipTextField setText:handler.connectedCamera.ipAddress];
    }
    
    // Put the text field back to its original width and hide the loading indicator
    [UIView animateWithDuration:0.2  delay:0.0 options:0 animations:^{ [_ipTextField setFrame:CGRectMake(_ipTextField.frame.origin.x, _ipTextField.frame.origin.y, _ipTextFieldWidth, _ipTextField.frame.size.height)]; } completion:nil];
    [_textFieldIndicator setHidden:YES];
    [_textFieldIndicator stopAnimating];
    
    _selectedRow = -1;
    if (_tableIndicator.superview != nil)
    {
        [_tableIndicator removeFromSuperview];
    }
    [_tableIndicator stopAnimating];
    [_cameraTable reloadData];
}

- (void)addCameraToList:(Camera *)camera
{
    if ([camera.cameraID length] > 0)
    {
        for (Camera *cam in _cameraList)
        {
            if ([cam.pin compare:camera.pin] == NSOrderedSame)
            {
                // Camera is already in list
                return;
            }
        }
        
        [_cameraList addObject:camera];
        [_cameraTable reloadData];
    }
}

- (void)refreshCameraList:(BOOL)refresh
{
    if (refresh)
    {
        LogInfo("Refreshing camera list");
        [_searchIndicator startAnimating];
        [_cameraList removeAllObjects];
        [NSThread detachNewThreadSelector:@selector(discoverCameras:) toTarget:self withObject:self];
    }
    else
    {
        [_searchIndicator stopAnimating];
    }
    
    [_searchLabel setHidden:!refresh];
    [_searchIndicator setHidden:!refresh];
    [_cameraTable setHidden:refresh];
    
    [_cameraTable reloadData];
}

- (void)keyboardWillShow:(CGRect)keyboardFrame
{
    const CGFloat navbarHeight = 64.0;
    const CGFloat textFieldBottom = navbarHeight + _ipTextField.frame.origin.y + _ipTextField.frame.size.height;
    const CGFloat keyboardTop = keyboardFrame.origin.y - keyboardFrame.size.height;
    const CGFloat padding = 10.0;
    
    if (textFieldBottom + padding > keyboardTop)
    {
        CGFloat offset = textFieldBottom + padding - keyboardTop;
        
        [UIView beginAnimations:nil context:NULL];
        [UIView setAnimationDuration:0.3];
        
        CGRect viewFrame = self.view.frame;
        viewFrame.size.height += keyboardFrame.size.height;
        viewFrame.origin.y -= offset;
        self.view.frame = viewFrame;
        
        [UIView commitAnimations];
    }
}

- (void)keyboardWillHide:(CGRect)keyboardFrame
{
    const CGFloat navbarHeight = 64.0;
    CGRect viewFrame = self.view.frame;
    
    // Ensures that the view has actually been moved up
    if (viewFrame.origin.y < navbarHeight)
    {
        [UIView beginAnimations:nil context:NULL];
        [UIView setAnimationDuration:0.3];
        
        viewFrame.origin.y = navbarHeight;
        viewFrame.size.height -= keyboardFrame.size.height;
        self.view.frame = viewFrame;
        
        [UIView commitAnimations];
    }
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    listeningForCameras = NO;
    listen = NO;
    _cameraList = [[NSMutableArray alloc] init];
    _tableIndicator = [[UIActivityIndicatorView alloc] initWithActivityIndicatorStyle:UIActivityIndicatorViewStyleWhite];
    _ipTextFieldWidth = _ipTextField.frame.size.width;
    
    [_cameraTable setDataSource:self];
    [_cameraTable setDelegate:self];
    [_ipTextField setDelegate:self];
    
    if ([_cameraTable respondsToSelector:@selector(setSeparatorInset:)])
    {
        [_cameraTable setSeparatorInset:UIEdgeInsetsZero];
    }
    
    
    // Makes it so tapping outside of the text field dismisses it
    UITapGestureRecognizer *tap = [[UITapGestureRecognizer alloc] initWithTarget:_ipTextField action:@selector(resignFirstResponder)];
    tap.cancelsTouchesInView = NO;
    [self.view addGestureRecognizer:tap];
    
    // Makes it so you can pull the table down to refresh the camera list
    UIRefreshControl *refreshControl = [[UIRefreshControl alloc] init];
    [refreshControl setTintColor:[UIColor whiteColor]];
    [refreshControl addTarget:self action:@selector(refreshFromPull:) forControlEvents:UIControlEventValueChanged];
    [_cameraTable addSubview:refreshControl];
    
    [self refreshCameraList:YES];
    [self setConnected:NO];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
}

- (BOOL)textFieldShouldReturn:(UITextField *)textField
{
    RCPHandler *handler = [RCPHandler instance];
    NSString *ipAddress = [_ipTextField text];
    
    [handler connectToCameraWithIP:ipAddress];
    
    // Shrink the text field width in order to make room for the loading indicator
    [UIView animateWithDuration:0.2  delay:0.0 options:0 animations:^{ [_ipTextField setFrame:CGRectMake(_ipTextField.frame.origin.x, _ipTextField.frame.origin.y, _ipTextFieldWidth - _textFieldIndicator.frame.size.width - 10.0, _ipTextField.frame.size.height)]; } completion:nil];
    [_textFieldIndicator startAnimating];
    [_textFieldIndicator setHidden:NO];
    
    int index = 0;
    for (Camera *camera in _cameraList)
    {
        if ([camera.ipAddress compare:ipAddress] == NSOrderedSame)
        {
            _selectedRow = index;
            [_cameraTable reloadData];
            break;
        }
        
        index++;
    }
    
    return YES;
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView
{
    return 1;
}

- (NSInteger)tableView:(UITableView*)tableView numberOfRowsInSection:(NSInteger)section
{
    return [_cameraList count];
}

- (UITableViewCell*)tableView:(UITableView*)tableView cellForRowAtIndexPath:(NSIndexPath*)indexPath
{
    RCPHandler *handler = [RCPHandler instance];
    Camera *camera = [_cameraList objectAtIndex:indexPath.row];
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:@"Camera"];
    
    UIImage *connectedImage = [[UIImage imageNamed:@"connected"] imageWithRenderingMode:UIImageRenderingModeAlwaysOriginal];
    UIGraphicsBeginImageContextWithOptions(CGSizeMake(connectedImage.size.width, connectedImage.size.height), NO, 0.0);
    UIImage *emptyImage = UIGraphicsGetImageFromCurrentImageContext();
    UIGraphicsEndImageContext();
    
    if (handler.connected && [handler.connectedCamera.pin compare:camera.pin] == NSOrderedSame)
    {
        // If connected, show the connected icon next to the connected camera
        cell.imageView.image = connectedImage;
    }
    else if (indexPath.row == _selectedRow)
    {
        // If connecting, put an empty placeholder image and a loading indicator next to the selected camera
        cell.imageView.image = emptyImage;
        [cell.imageView addSubview:_tableIndicator];
        [_tableIndicator startAnimating];
    }
    else
    {
        // If the camera in this row is not the camera the app is connected to/not the camera the user selected, put an empty placeholder image next to the camera in order to indent it
        cell.imageView.image = emptyImage;
    }
    
    
    NSString *interface = [[NSString alloc] init];
    
    switch (camera.interface)
    {
        case RCP_INTERFACE_UNKNOWN:
            interface = @"Unknown";
            break;
            
        case RCP_INTERFACE_BRAIN_SERIAL:
            interface = @"Brain Serial";
            break;
            
        case RCP_INTERFACE_BRAIN_GIGABIT_ETHERNET:
            interface = @"Brain GigE";
            break;
            
        case RCP_INTERFACE_REDLINK_BRIDGE:
            interface = @"REDLINK BRIDGE";
            break;
            
        case RCP_INTERFACE_BRAIN_WIFI:
            interface = @"WiFi";
            break;
            
        default:
            interface = @"???";
            break;
    }
    
    [cell.textLabel setText:[NSString stringWithFormat:@"%@ (%@)", camera.cameraID, camera.type]];
    [cell.detailTextLabel setText:[NSString stringWithFormat:@"%@ | %@", camera.pin, interface]];
    
    return cell;
}

- (void)tableView:(UITableView*)tableView didSelectRowAtIndexPath:(NSIndexPath*)indexPath
{
    [tableView deselectRowAtIndexPath:indexPath animated:YES];
    
    RCPHandler *handler = [RCPHandler instance];
    Camera *camera = [_cameraList objectAtIndex:indexPath.row];
    
    if ([camera.ipAddress compare:handler.connectedCamera.ipAddress] == NSOrderedSame)
    {
        [handler dropConnection];
    }
    else
    {
        [handler connectToCameraWithIP:camera.ipAddress];
        
        _selectedRow = indexPath.row;
        [_cameraTable reloadData];
    }
}

- (void)refreshFromPull:(UIRefreshControl *)refreshControl
{
    [self refreshCameraList:YES];
    [refreshControl endRefreshing];
}

- (void)discoverCameras:(id)obj
{
    ConnectionViewController *controller = (ConnectionViewController *)obj; // Refers to "self"
    
    if (controller == nil)
    {
        LogError("Invalid parameter specified");
        return;
    }
    
    if (!controller.listeningForCameras)
    {
        controller.listen = YES;
        
        [NSThread detachNewThreadSelector:@selector(listenForCameras:) toTarget:controller withObject:controller];
        usleep(UDP_DISCOVERY_SLEEP_TIME_US);
    }
    
    
    rcp_discovery_start(broadcastMessage, (__bridge void*)controller);
    
    for (size_t i = 0; i < RCP_DISCOVERY_STEP_LOOP_COUNT; i++)
    {
        usleep(RCP_DISCOVERY_STEP_SLEEP_MS * UDP_DISCOVERY_SLEEP_TIME_US);
        rcp_discovery_step();
    }
    
    usleep(RCP_DISCOVERY_STEP_SLEEP_MS * UDP_DISCOVERY_SLEEP_TIME_US);
    
    controller.listen = NO;
    
    
    rcp_discovery_cam_info_list_t *cam_list = rcp_discovery_get_list();
    const rcp_discovery_cam_info_list_t *cur = cam_list;
    
    while (cur)
    {
        Camera *camera = [[Camera alloc] init];
        camera.cameraID = [NSString stringWithFormat:@"%s", cur->info.id];
        camera.pin = [NSString stringWithFormat:@"%s", cur->info.pin];
        camera.version = [NSString stringWithFormat:@"%s", cur->info.version];
        camera.type = [NSString stringWithFormat:@"%s", cur->info.type];
        camera.ipAddress = [NSString stringWithFormat:@"%s", cur->ip_address];
        camera.interface = cur->info.rcp_interface;
        
        [controller addCameraToList:camera];
        
        cur = cur->next;
    }
    
    RCPHandler *handler = [RCPHandler instance];
    if (handler.connected)
    {
        // If already connected, ensures that the camera the user is connected to is in the list, even if the user manually entered its IP address
        Camera *camera = [[Camera alloc] initWithCamera:handler.connectedCamera];
        [controller addCameraToList:camera];
    }
    
    [controller refreshCameraList:NO];
    
    rcp_discovery_free_list(cam_list);
    rcp_discovery_end();
}

- (void)listenForCameras:(id)obj
{
    ConnectionViewController *controller = (ConnectionViewController *)obj; // refers to "self"
    
    if (controller == nil)
    {
        LogError("Invalid parameter specified");
        return;
    }
    
    int sock;
    struct sockaddr_in addr;
    unsigned short port = 1112;
    struct timeval tv;
    
    if ((sock = socket(PF_INET, SOCK_DGRAM, IPPROTO_UDP)) < 0)
    {
        LogError("A socket error occurred");
        return;
    }
    
    
    // If recvfrom() doesn't receive any data within UDP_LISTEN_TIMEOUT seconds, it returns
    tv.tv_sec = UDP_LISTEN_TIMEOUT;
    tv.tv_usec = 0;
    if (setsockopt(sock, SOL_SOCKET, SO_RCVTIMEO, (char *)&tv, sizeof(struct timeval)) < 0)
    {
        close(sock);
        LogError("Unable to set UDP read timeout socket option");
        return;
    }
    
    
    addr.sin_family = AF_INET;
    addr.sin_addr.s_addr = INADDR_ANY;
    addr.sin_port = htons(port);
    
    if (bind(sock, (const struct sockaddr*)&addr, sizeof(addr)) < 0)
    {
        close(sock);
        LogError("Failed to bind socket");
        return;
    }
    
    
    LogInfo("Listening for cameras");
    controller.listeningForCameras = YES;
    
    while (controller.listen)
    {
        char packet_data[4096];
        char ipv4_pretty[32];
        char * from_ipv4 = NULL;
        unsigned int maximum_packet_size = sizeof(packet_data);
        ssize_t received_bytes;
        
        struct sockaddr_in from;
        socklen_t len = sizeof(from);
        
        received_bytes = recvfrom(sock, (char *)packet_data, maximum_packet_size, 0, (struct sockaddr *)&from, &len);
        
        if (received_bytes <= 0)
        {
            break;
        }
        
        if (from.sin_family == AF_INET)
        {
            strlcpy(ipv4_pretty, inet_ntoa(from.sin_addr), sizeof(ipv4_pretty));
            from_ipv4 = ipv4_pretty;
        }
        
        rcp_discovery_process_data(packet_data, received_bytes, from_ipv4);
    }
    
    close(sock);
    controller.listeningForCameras = NO;
    LogInfo("No longer listening for cameras");
    return;
}

void broadcastMessage(const char *data, size_t len, void *user_data)
{
    int sock, broadcastPermission;
    unsigned short port = UDP_PORT;
    struct ifaddrs *interfaces = NULL;
    struct ifaddrs *interface = NULL;
    
    if ((sock = socket(PF_INET, SOCK_DGRAM, IPPROTO_UDP)) < 0)
    {
        LogError("Unable to create UDP socket");
        return;
    }
    
    broadcastPermission = 1;
    if (setsockopt(sock, SOL_SOCKET, SO_BROADCAST, (void *) &broadcastPermission, sizeof(broadcastPermission)) < 0)
    {
        close(sock);
        LogError("Unable to set UDP broadcast socket options");
        return;
    }
    
    if (getifaddrs(&interfaces) == 0)
    {
        interface = interfaces;
        
        // By iterating over each interface, it's guaranteed that the message will be broadcast over the wireless interface (and not just the cellular interface)
        while (interface)
        {
            struct sockaddr *temp_addr = NULL;
            if (interface->ifa_broadaddr && (interface->ifa_flags & IFF_BROADCAST))
            {
                temp_addr = interface->ifa_broadaddr;
            }
            else if (interface->ifa_dstaddr && (interface->ifa_flags & IFF_POINTOPOINT))
            {
                temp_addr = interface->ifa_dstaddr;
            }
            
            if (temp_addr)
            {
                struct sockaddr_in addr;
                memset(&addr, 0, sizeof(addr));
                addr.sin_family = temp_addr->sa_family;
                addr.sin_addr = ((struct sockaddr_in *)temp_addr)->sin_addr;
                addr.sin_port = htons(port);
                
                if (sendto(sock, data, len, 0, (struct sockaddr *)&addr, sizeof(addr)) == -1)
                {
                    LogError("Unable to broadcast message");
                }
            }
            
            interface = interface->ifa_next;
        }
        
        freeifaddrs(interfaces);
        close(sock);
    }
}

@end
