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

package com.red.rcp;

import static java.lang.System.out;
import com.red.common.HandleWrapper;

import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketException;

/**
 * <code>RCPDiscovery</code> is the class that performs camera discovery on the network.
 * The class that instantiates an <code>RCPDiscovery</code> object must also be a
 * {@link RCPDiscoveryListener}. This is because all the callbacks defined
 * in {@link RCPDiscoveryListener} are delegated to the parent class. This
 * allows the parent class to handle the camera callbacks directy in its
 * own context. The delegation mechanism is thread safe and allows for the
 * callbacks to be posted to UI threads.
 * <p>
 * This class implements private JNI calls to and from the RCP SDK. JNI
 * is the mechanism for wrapping the RCP SDK in Java.
 * 
 */
public class RCPDiscovery implements RCPDiscoveryListener {
	static {
		// Load Native (NDK) library (librcplib.so)
		System.loadLibrary("rcp");
	}
			
	private static final String TAG = "RCPDiscovery";
	private static final int DISCOVERY_PORT = 1112;
	private HandleWrapper m_Handler;
	private RCPDiscoveryListener m_DelegateListener;
	private Thread m_discoveryThread;
	private Thread m_listenerThread;
	private boolean m_keepListening;
	private DatagramSocket m_listenerSocket;
	private DatagramSocket m_discoverySocket;

	
	// Native (NDK) functions defined in C/C++, prototypes auto-generated
	private native void jni_init();
	private native void jni_startDiscovery();
	private native void jni_processResponse(byte[] msg, String ipAddr);
	
	/**
	 * Class constructor specifying the parent {@link RCPDiscoveryListener}.
	 * 
	 * @param pListener		parent {@link RCPDiscoveryListener}
	 */
	public RCPDiscovery(RCPDiscoveryListener pListener) {
		m_Handler = new HandleWrapper();
		m_DelegateListener = pListener;
		
		// Initialize the JNI environment
		jni_init();
			
	}
 
	/**
	 * Starts the camera discovery process. Once the discovery
	 * process has been started, it should not be started again until
	 * the {@link onCameraDiscoveryUpdate} callback has been invoked.
	 */
	public void startDiscovery() {

		m_keepListening = true;
		
		// Thread to perform broadcast discovery
		m_discoveryThread = new Thread(new Runnable() {
	        public void run() {
	        	jni_startDiscovery();
	        	m_keepListening = false;
	        	m_discoverySocket.close();
	        	m_listenerSocket.close();
	        }
		});
		
		// Thread to listen to camera responses
		m_listenerThread = new Thread(new Runnable() {
			public void run() {
				receiveReply();
			}
		});
		
		m_listenerThread.start();
		m_discoveryThread.start();
	}

	/**
	 * Invoked when camera discovery is complete. The method is delegated
	 * to the parent listener.
	 * 
	 * @param cameras	list of cameras discovered on the network
	 * 
	 * @see RCPDiscoveryListener#onCameraDiscoveryUpdate
	 */
	@Override
	public void onCameraDiscoveryUpdate(final String cameras) {
		m_Handler.post(new Runnable() {
			public void run() {
				//System.out.println("RCPDiscovery: Cameras: " + cameras);
				m_DelegateListener.onCameraDiscoveryUpdate(cameras);
			}
		});		
	}
	
	private void receiveReply() {

		try {
			m_listenerSocket = new DatagramSocket(DISCOVERY_PORT, InetAddress.getByName("0.0.0.0"));
			try {
				while (m_keepListening == true) {
					 byte[] data = new byte[1024];
					 DatagramPacket rcvPacket = new DatagramPacket(data, data.length);
					 m_listenerSocket.receive(rcvPacket);
					 // Send to JNI for processing by RCP SDK
					 jni_processResponse(rcvPacket.getData(), rcvPacket.getAddress().toString().split("/")[1]);
				 }
			} catch (Exception e) {
				if (m_keepListening == true) {
					e.printStackTrace();
				}
			}		
		} catch (Exception e) {
			e.printStackTrace();
		}		
	}
	
	private void onBroadcastRequest(byte[] msg) {

		// Create a broadcast socket and send msg
		try {			
			m_discoverySocket = new DatagramSocket();
			m_discoverySocket.setBroadcast(true);
			try {
				 DatagramPacket sendPacket = new DatagramPacket(msg, msg.length, InetAddress.getByName("255.255.255.255"), DISCOVERY_PORT);
				 m_discoverySocket.send(sendPacket);
			} catch (Exception e) {
				e.printStackTrace();
			}

		} catch (Exception e) {
			e.printStackTrace();
		}	
	}
}
