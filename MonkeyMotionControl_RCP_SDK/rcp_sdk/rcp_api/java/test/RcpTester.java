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

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketException;

import com.red.rcp.*;

public class RcpTester implements RCPDiscoveryListener, RCPCameraListener, Runnable {
	
	private final String TAG = "RCP Java Test: ";
	private static final String PATTERN = 
	        "^([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\." +
	        "([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\." +
	        "([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\." +
	        "([01]?\\d\\d?|2[0-4]\\d|25[0-5])$";
	
	RCPDiscovery m_disco;
	RCPCamera m_camera;
	boolean m_discoveryUpdate = false;
	List<String> m_cameras;
	Pattern m_pattern;
	
	@Override
	public void run() {

		boolean keepDiscoverying = true;
		String pin = new String("");
		String ipAddress = new String("");
		
		m_cameras = new ArrayList<String>();
		m_pattern = Pattern.compile(PATTERN);
				
		//--------------------------------------------------------------------
		// Camera Discovery
		//--------------------------------------------------------------------
		
		// Create the discovery object and start camera discovery
		m_disco = new RCPDiscovery(this); 	

		do {
			m_disco.startDiscovery();
		
			try {
				while (m_discoveryUpdate == false) {
					java.lang.Thread.sleep(250);
				}
			} catch (Exception e) {
				System.out.println(TAG + "Error sleeping: " + e.toString());
			}
			m_discoveryUpdate = false;
			
			System.out.flush();
			
			System.out.println("Choose Connection:");
			for (int ii = 0; ii < m_cameras.size(); ii++) {
				System.out.printf("  %d. %s\n", ii+1, m_cameras.get(ii));
			}
			System.out.printf("  %d. Enter manual IP Address\n\n", m_cameras.size()+1);
			System.out.printf("Enter Choice >> ");
			
			  //  open up standard input
			  BufferedReader br = new BufferedReader(new InputStreamReader(System.in));
			
			  String choice = null;
			
			  try {
			     choice = br.readLine();
			  } catch (IOException ioe) {
			     System.out.println("IO error trying to read choice!");
			     System.exit(1);
			  }
			  
			  
			  int index = Integer.parseInt(choice) - 1;
			  if (m_cameras.size() == index) {
				  
				  // Manual IP			
				  System.out.printf("Enter IP Address >> ");
				  String ip = null;		
				  try {
				     ip = br.readLine();
				  } catch (IOException ioe) {
				     System.out.println("IO error trying to read choice!");
				     System.exit(1);
				  }
				  
				  if (validateIP(ip)) {
					  System.out.println("Valid IP");
					  keepDiscoverying = false;
					  ipAddress = ip;
					  pin = "123-456-789";  // Must be a unique 9 digit pin
				  } else {
					  System.out.println("Invalid IP");
				  }
				  
			  } else if (m_cameras.size() > 0 && index <= m_cameras.size() && index >= 0){
				  
				  String[] temp = m_cameras.get(index).split(":");
				  pin = temp[1];
				  ipAddress = temp[5];
				  keepDiscoverying = false;
				  
			  } else {
				  System.out.println("Invalid choice.");
			  }
			  
		} while (keepDiscoverying);
		
		//--------------------------------------------------------------------
		// Connect to a RED camera
		//--------------------------------------------------------------------
		
		m_camera = new RCPCamera(this);
		String appName = "RcpTester";
		String appVer = "1.0";

		int status = m_camera.open(pin, ipAddress, appName, appVer, null);
		if (0 == status) {
			System.out.println(TAG + "open successful");
		} else {
			System.out.println(TAG + "open FAILED");
			System.exit(1);
		}
		
		// Sleep while data is streaming 
		sleep(2000);
		
		//--------------------------------------------------------------------
		// Run tests
		//--------------------------------------------------------------------
		
		//testNotifications(m_camera);
		//testStats(m_camera);
		//testParam(m_camera, "RCP_PARAM_RECORD_HDR_MODE");
		//testParam(m_camera, "RCP_PARAM_MONITOR_PRIORITY_LIST");
		testLists(m_camera, "RCP_PARAM_MONITOR_PRIORITY_LIST",
				"Brain LCD|Brain HDMI|Brain EVF|Rear LCD|Rear EVF|Brain HD-SDI|Rear PVW|Rear PGM|Meizler HD-SDI|",
				"0|2|1|4|5|3|6|7|8|");
		sleep(5000);
		testLists(m_camera, "RCP_PARAM_MONITOR_PRIORITY_LIST",
				"Meizler HD-SDI|Brain LCD|Brain HDMI|Brain EVF|Rear LCD|Rear EVF|Brain HD-SDI|Rear PVW|Rear PGM|",
				"8|0|2|1|4|5|3|6|7|");
		//testParam(m_camera, "RCP_PARAM_SLATE_CAMERA_OPERATOR");
		//testParam(m_camera, "RCP_PARAM_ISO");
				
		//--------------------------------------------------------------------
		// Close connection
		//--------------------------------------------------------------------
		
		System.out.println(TAG + "Closing camera connection (20 seconds has elapsed)");
		m_camera.close();
		System.out.println(TAG + "Thread exiting...");
	}
	
	private void testNotifications(RCPCamera camera) {
		camera.notificationGet();	// get current notification from camera
		sleep(2000);				// sleep to allow for the callback to come
		String notificationID = commandLinePrompt("Enter notification ID: ");
		camera.notificationResponse(notificationID, 0);	// hardcoded to respond with 0 (close)
	}
	
	private void testStats(RCPCamera camera) {
		int tx_packets = camera.getCameraConnectionStats(0);
		int tx_bytes = camera.getCameraConnectionStats(1);
		int rx_packets = camera.getCameraConnectionStats(2);
		int rx_bytes = camera.getCameraConnectionStats(3);
		System.out.println(TAG + "tx_packets = " + tx_packets + ", tx_bytes = " + tx_bytes + 
				", rx_packets = " + rx_packets + ", rx_bytes = " + rx_bytes);
	}
	
	private void testParam(RCPCamera camera, String param) {
		camera.getParameter(param);
		camera.getstatus(param);
		camera.getList(param);
		sleep(2000);				// sleep to allow for callbacks to come
		for(int i = 0; i < 10; ++i) { 
			System.out.println("param = " + param + "; property = " + i + 
					"; supported = " + camera.getPropertyIsSupported(param, i));			
		}
	}
	
	private void testLists(RCPCamera camera, String param, String listStrings, String listValues) {
		// if param supports get list
		if(1 == camera.getPropertyIsSupported(param, 1)) {
			camera.getList(param);
			sleep(2000);			// sleep to allow for callback to come
		}
		else {
			return;
		}
		// if param supports set list
		if(1 == camera.getPropertyIsSupported(param, 7)) {
			camera.setList(param, listStrings, listValues);
			sleep(2000);
			camera.getList(param);
			sleep(2000);			// sleep to allow for callback to come			
		}
	}
	
	private String commandLinePrompt(final String promptText) {
		BufferedReader br = new BufferedReader(new InputStreamReader(System.in));
		
		System.out.printf("%s", promptText);
		String userInput = null;		
		try {
			userInput = br.readLine();
		} catch (IOException ioe) {
		   System.out.println("IO error trying to read choice!");
		   System.exit(1);
		}
		return userInput;
	}
	
	public boolean validateIP(final String ip) {
	      Matcher matcher = m_pattern.matcher(ip);
	      return matcher.matches();
	}
	
	public void sleep(int ms) {
		try {
			java.lang.Thread.sleep(ms);
		} catch (Exception e) {
			System.out.println(TAG + "Error sleeping: " + e.toString());
		}
	}
	
	@Override
	public void onUpdate(String id, String param) {
//		System.out.printf("%s onUpdate(): CameraId = %s, param = %s\n", TAG, id, param);
	}

	@Override
	public void onUpdateInt(String id, String param, int value) {
//		System.out.printf("%s onUpdateInt(): CameraId = %s, param = %s, value = %d\n", TAG, id, param, value);
	}

	@Override
	public void onUpdateString(String id, String param) {
//		System.out.printf("%s onUpdateString(): CameraId = %s, param = %s\n", TAG, id, param);
	}
	
	@Override
	public void onUpdateEditInfo(String id, String editInfo) {
//		System.out.printf("%s onUpdateEditInfo(): CameraId = %s, editInfo = %s\n", TAG, id, editInfo);
	}

	@Override
	public void onUpdateStringEditInfo(String id, String paramName, int[] intParams, String[] strParams) {
//		System.out.printf("%s onUpdateStringEditInfo(): CameraId = %s, paramName = %s, numInts = %d, numStr = %d\n", TAG, id, paramName, intParams.length, strParams.length);
	}

	@Override
	public void onUpdateClipList(String id, int status, String clipList) {

		if (1 == status) {
			String[] list = clipList.split(";");
			System.out.printf("%s onUpdateClipList(): CameraId = %s, status = %d\n", TAG, id, status);
			System.out.printf("    Clip List:\n");
			for (int ii = 0; ii < list.length; ii++) {
				System.out.printf("        %s\n", list[ii]);
			}
		} else {
			System.out.printf("%s onUpdateClipList(): CameraId = %s, status = %d\n", TAG, id, status);
		}
	}

	@Override
	public void onUpdateListStrings(String id, String listHeader,
			String listFlags, String[] listStrings, int[] listValues) {
		System.out.printf("%s onUpdateListStrings(): CameraId = %s, listHeader = %s, listFlags = %s\n", 
				TAG, id, listHeader, listFlags);
	}

	@Override
	public void onUpdateHistogram(String id, int[] red, int[] green,
			int[] blue, int[] luma, int bottom_clip, int top_clip,
			String text) {
		//System.out.printf("%s onHistogramUpdate(): CameraId = %s\n", TAG, id);
	}

	@Override
	public void onConnectionError(String id, String msg) {
		System.out.printf("%s onConnectionError(): CameraId = %s, Connection ERROR: msg = %s\n", TAG, id, msg);
	}

	@Override
	public void onStateChange(String id, String param) {
		System.out.printf("%s onStateChange(): CameraId = %s, param = %s\n", TAG, id, param);
		
		// Get menu tree
		if(1 == m_camera.menuIsSupported())
		{
			System.out.println(TAG + "Menu is supported");
			m_camera.menuGetChildren(0);
		}
		else
		{
			System.out.println(TAG + "Menu is not supported");
		}
	}
	
	@Override
	public void onStatusUpdate(String id, String param, int isEnabled, int isEnabledValid, int isSupported, int isSupportedValid) {
		System.out.printf("%s onStatusUpdate(): CameraId = %s, param = %s, value = %d\n", TAG, id, param, isEnabled);
	}
	
	@Override
	public void onNotificationUpdate(String id, String nID, String title, String message, String notificationInfo) {
		System.out.printf("%s onNotificationUpdate(): CameraId = %s, notificaation ID = %s\n", TAG, id, nID);
		System.out.printf("%s onNotificationUpdate(): title = %s\n", TAG, title);
		System.out.printf("%s onNotificationUpdate(): message = %s\n", TAG, message);
		System.out.printf("%s onNotificationUpdate(): notificationInfo = %s\n", TAG, notificationInfo);
	}

	@Override
	public void onAudioVuUpdate(String id, String param) {
//		System.out.printf("%s onAudioVuUpdate(): CameraId = %s, param = %s\n", TAG, id, param);
	}
	
	@Override
    public void onMenuNodeListUpdate(String id, int nodeId, String[] childList, String[] ancestorList) {
		System.out.printf("%s onMenuNodeListUpdate(): current node = %d\n", TAG, nodeId);
		System.out.printf("    Child List:\n");
		for (int ii = 0; ii < childList.length; ii++) {
			System.out.printf("        %s\n", childList[ii]);
		}
		System.out.printf("    Ancestor List:\n");
		for (int ii = 0; ii < ancestorList.length; ii++) {
			System.out.printf("        %s\n", ancestorList[ii]);
		}
	}

    @Override
    public void onMenuNodeStatusUpdate(String id, int nodeId, int isEnabled, int isEnabledValid, int isSupported, int isSupportedValid) {
		System.out.printf("%s onMenuNodeStatusUpdate(): nodeId = %d isEnabled = %d isEnabledValid = %d\n", TAG, nodeId, isEnabled, isEnabledValid);
    }

	@Override
	public void onUpdateTag(String id, String param) {
		System.out.printf("%s onUpdateString(): CameraId = %s, state = %s\n", TAG, id, param);		
	}

	@Override
	public void onCameraDiscoveryUpdate(String cameras) {
				
		System.out.printf("%s onCameraDiscoveryUpdate(): %s", TAG, cameras);
		m_cameras.clear();
		if(!cameras.isEmpty()) {
			String[] cameraList = cameras.split(";");
			for (int ii = 0; ii < cameraList.length; ii++) {
				m_cameras.add(cameraList[ii]);
			}
			
		}
		m_discoveryUpdate = true;
	}
	
	@Override
	public void onCameraInfo(String id, String camInfo) {
		System.out.printf("%s onCameraInfo(): CameraId = %s, camInfo = %s\n", TAG, id, camInfo);
	}
	
	@Override
    public void onRftpStatusUpdate(String id, byte[] uuid, int rftpType, int rftpError, int percentComplete, String[] dirList) {
		System.out.printf("%s onRftpStatusUpdate(): CameraId = %s, rftpType = %d, rftpError = %d percent = %d\n", TAG, id, rftpType, rftpError, percentComplete);
	}
}
