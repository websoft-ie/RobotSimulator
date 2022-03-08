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

import java.util.concurrent.locks.ReentrantLock;
import java.util.concurrent.LinkedBlockingQueue;
import com.red.common.HandleWrapper;

/**
 * <code>RCPCamera</code> is the class that opens, maintains and closes a connection
 * to a specific camera. The class provides methods for setting camera
 * parameters as well as callbacks for receiving camera status.
 * <p>
 * The class that instantiates a <code>RCPCamera</code> object must also be a
 * {@link RCPCameraListener}. This is because all the callbacks defined
 * in {@link RCPCameraListener} are delegated to the parent class. This
 * allows the parent class to handle the camera callbacks directly in its
 * own context. The delegation mechanism is thread safe and allows for the
 * callbacks to be posted to UI threads.
 * <p>
 * This class implements private JNI calls to and from the RCP SDK. JNI
 * is the mechanism for wrapping the RCP SDK in Java.
 * 
 */
public class RCPCamera implements RCPCameraListener {
	static {
		// Load Native (NDK) library (librcp.so)
		System.loadLibrary("rcp");
	}
	public final static int RCP_UUID_LEN = 41;
	
	private final static String TAG = "RCPCamera";

	private String m_ID;
	private String m_IP;
	private HandleWrapper m_Handler;
	private RCPCameraListener m_DelegateListener;
	private Thread m_listenerThread;
	private Thread m_writeThread;
	private Thread m_openThread;
	private LinkedBlockingQueue<byte[]> m_writeQueue;
	private boolean m_keepListening;
	private int m_openStatus;
	private final ReentrantLock m_mutex = new ReentrantLock();	
	private RCPCommInterface m_commHandle = null;

	// Native (NDK) functions defined in C/C++, prototypes auto-generated
	private native void jni_init					();
	private native int  jni_open					(String id, String client_name, String client_version, String client_user);
	private native void jni_close					(String id);
	private native int  jni_processData				(String id, byte[] msg, int nbytes);
	private native void jni_toggleRecordState		(String id);
	private native void jni_setParameter			(String id, String parameterName, int value);
	private native void jni_setParameterString		(String id, String parameterName, String value);
	private native void jni_getParameter			(String id, String parameterName);
	private native void jni_getList					(String id, String listName);
	private native void jni_setList					(String id, String listName, String listStrings, String listValues);
	private native String jni_getParameterLabel		(String id, String parameterName);
	private native int jni_getIsSupported			(String id, String parameterName);
	private native int jni_getPropertyIsSupported	(String id, String parameterName, int propertyType);
	private native void jni_setParameterUnsigned	(String id, String parameterName, long value);
	private native void jni_send					(String id, String parameterName);
	private native String jni_getAPIVersion			();
	private native int jni_menuIsSupported			(String id);
	private native int jni_menuGetChildren			(String id, int menuNodeId);
	private native int jni_menuNodeStatusIsSupported(String id);
	private native int jni_menuGetNodeStatus		(String id, int menuNodeId);	
	private native int jni_notificationGet			(String id);
	private native int jni_notificationTimeout		(String id, String notificationId);
	private native int jni_notificationResponse		(String id, String notificationId, int response);
	private native int jni_getCameraConnectionStats	(String id, int statsType);
	private native int jni_getStatus				(String id, String parameterName);
	private native String jni_getParameterName		(String id, int parameterId);
	private native int jni_setPeriodicEnable		(String id, String parameterName, boolean enable);

	private native int jni_rftpIsSupported			(String id);
	private native int jni_rftpSendFile				(String id, String localFile, String cameraFile, boolean compress, byte[] uuid_out);
	private native int jni_rftpRetrieveFile			(String id, String localFile, String cameraFile, int maxFileSize, boolean compressionAllowed, byte[] uuid_out);
	private native int jni_rftpAbortTransfer		(String id, byte[] uuid_in);
	private native int jni_rftpDeleteFile			(String id, String cameraFile, byte[] uuid_out);
	private native int jni_rftpDirList				(String id, String path, byte[] uuid_out);
	
	/**
	 * Class constructor specifying the parent {@link RCPCameraListener}
	 * 
	 * @param pListener		parent {@link RCPCameraListener}
	 */
	public RCPCamera(RCPCameraListener pListener) {
		m_Handler = new HandleWrapper();
		m_DelegateListener = pListener;

		// Default communication handler is NetworkComm
		m_commHandle = (RCPCommInterface) new NetworkComm();
		
		jni_init();			
			
	}
	
	public void registerCommInterface(RCPCommInterface commInterface) {
		m_commHandle = commInterface;
	}

	/**
	 * Opens a camera connection. The camera connection is specified
	 * by the camera PIN and the IP address. If the PIN is unknown,
	 * a unique 9 digit pin may be used such as "000-000-001". This 
	 * must be unique for each unique camera connection. It is recommended
	 * that the PIN provided from camera discovery is used as it is
	 * the actual PIN from the camera and is guaranteed unique.
	 * 
	 * @param pin				unique 9 digit pin of the camera with format "102-456-789"
	 * @param ipaddress			IP address of camera (e.g. "192.168.1.144")
	 * @param client_name		A string indicating the name of the client, usually the app name, e.g. "MyAndroidApp"
	 * @param client_version	A string indicating the version of the client, e.g. "1.2"
	 * @param client_user		Optional unique user defined string, null if not used
	 * @return 					<code>0</code> if connection was opened successfully;
	 * 							<code>-1</code> if the connection failed
	 */
	public int open(String pin, String ipaddress, final String client_name, final String client_version, final String client_user) {
		int status = 0;
		// Use the Camera PIN as the ID for the camera object
		m_ID = pin;
		m_IP = ipaddress;
		m_openStatus = 0;

		// Thread to listen to camera data
		m_listenerThread = new Thread(new Runnable() {
			public void run() {
				processData();
			}
		});

		// Message queue for sending data to camera
		m_writeQueue = new LinkedBlockingQueue<byte[]>();

		// Thread to handle sending data to camera
		m_writeThread = new Thread(new Runnable() {
			public void run() {
				processWrites();
			}
		});

		m_openThread = new Thread(new Runnable() {
			public void run() {
				try {
					m_mutex.lock();
					try {
						m_openStatus = 0;
						try {
							m_commHandle.open(m_IP);
							m_keepListening = true;
							m_listenerThread.start();
							m_writeThread.start();
							m_openStatus = jni_open(m_ID, client_name, client_version, client_user);

						} catch (Exception e) {
							e.printStackTrace();
							m_openStatus = -1;
						}
					} finally {
						m_mutex.unlock();
					}
				} catch(Exception e) {
					System.out.println("RCP SDK: Error aquiring mutex in open()");
					e.printStackTrace();
					m_openStatus = -1;
				}
			}
		});	
		
		try {
			m_openThread.start();
			m_openThread.join();
		} catch (Exception e) {
			System.out.println("RCP SDK: Exception thrown in open(), trying to start and join openThread");
			e.printStackTrace();
			m_openStatus = -1;
		}
		
		return m_openStatus;
	}
	
	/**
	 * Closes a camera connection.
	 */
	public void close() {
		try { 
			m_mutex.lock();
			try {
				try {
					jni_close(m_ID);
					m_keepListening = false;
					m_commHandle.close();
					m_writeThread.interrupt();
				} catch (Exception e) {
					e.printStackTrace();
				}
			} finally {
				m_mutex.unlock();
			}
		} catch(Exception e) {
			System.out.println("RCP SDK: Error aquiring mutex in close()");
			e.printStackTrace();
		}
		
		

	}
	
	private void processData() {
		byte[] data = new byte[1024];
		int nbytes;
		
		try {
			while (m_keepListening == true) {
				 nbytes = m_commHandle.read(data, 1024);
				 if (nbytes > 0) {
					 jni_processData(m_ID, data, nbytes);
				 } else {
					 if (nbytes < 0) {
						 //System.out.println("RCPCamera::processData(): READ ERROR: nbytes = " + Integer.toString(nbytes));
					 }
				 }
			 }
		} catch (Exception e) {
			if (m_keepListening == true) {
				e.printStackTrace();
			}
		}
	}

	private void processWrites() {
		byte[] msg;
		try {
			while (m_keepListening == true) {
				// Blocking call, will wait until we have something in the queue
				msg = m_writeQueue.take();
				m_commHandle.write(msg);
			}
		}
		catch (InterruptedException e) {
			System.out.println("RCPCamera::processWrites(): exiting thread");
			m_writeQueue.clear();
		}
		catch (Exception e) {
			if (m_keepListening == true) {
				System.out.println("RCPCamera::processWrites(): unhandled exception");
				e.printStackTrace();
			}
		}
	}

	private int sendToCamera(byte[] msg) {
		int status = 0;
		try {
			// offer adds the element to the end of the queue if there is space.
			// It doesn't block and returns immediately if successful or not
			status = m_writeQueue.offer(msg) ? 0 : -1;
		}
		catch (NullPointerException e) {
			e.printStackTrace();
			status = -1;
		}
		return status;
	}

	/**
	 * Returns the camera ID for the object instance. This ID is
	 * the camera PIN, and uniquely identifies the {@link RCPCamera} object. This is
	 * used by the parent class to identify a specific callback associated
	 * with a specific {@link RCPCamera} object.
	 */
	public String getID() {
		return m_ID;
	}
	/**
	 * Toggles the record state of the camera.
	 */
	public void toggleRecordState() {
		jni_toggleRecordState(m_ID);
	}
	
	/**
	 * Send a argument-less RCP parameter to the camera.
	 * 
	 * @param parameterName		name of the RCP parameter
	 */
	public void setParameter(String parameterName) {
		jni_send(m_ID, parameterName);
	}
	
	/**
	 * Sets an integer parameter on the camera.
	 * 
	 * @param parameterName		name of the RCP parameter (e.g. "RCP_PARAM_SATURATION")
	 * @param value				value to set (e.g. 1200)
	 */
	public void setParameter(String parameterName, int value) {
		jni_setParameter(m_ID, parameterName, value);
	}
	
	/** 
	 * Sets a string parameter on the camera.
	 * 
	 * @param parameterName		name of the RCP parameter (e.g. "RCP_PARAM_SLATE_SCENE")
	 * @param value				value to set (e.g. "Woods")
	 */
	public void setParameterString(String parameterName, String value) {
		jni_setParameterString(m_ID, parameterName, value);
	}
	
	/** 
	 * Gets a parameter from the camera.
	 * 
	 * @param parameterName		name of the RCP parameter (e.g. "RCP_PARAM_SATURATION")
	 */
	public void getParameter(String parameterName) {
		jni_getParameter(m_ID, parameterName);
	}
	
	/** 
	 * Gets a list from the camera.
	 * 
	 * @param listName		name of the RCP parameter (e.g. "RCP_PARAM_ISO")
	 */
	public void getList(String listName) {
		jni_getList(m_ID, listName);
	}
	
	/** 
	 * Sets a list in the camera.
	 * 
	 * @param listName		name of the RCP parameter (e.g. "RCP_PARAM_ISO")
	 * @param listStrings	list of strings to be used to set the RCP parameter's list
	 * @param listValues	list of values to be used to set the RCP parameter's list
	 */
	public void setList(String listName, String listStrings, String listValues) {
		jni_setList(m_ID, listName, listStrings, listValues);
	}
	
	/**
	 * Gets a camera parameter label. This is the label that should
	 * be used for display in a UI.
	 * 
	 * @param parameterName		name of the RCP parameter (e.g. "RCP_PARAM_ISO")
	 */
	public String getParameterLabel(String parameterName) {
		return (jni_getParameterLabel(m_ID,parameterName));
	}
	
	/**
	 * Check if menu tree is supported by connected camera
	 */
	public int menuIsSupported() {
		return (jni_menuIsSupported(m_ID));
	}
	
	/**
	 * Requests menu tree children for given parent mode
	 * 
	 * @param menuNodeId		ID of the parent node
	 */
	public int menuGetChildren(int menuNodeId) {
		return (jni_menuGetChildren(m_ID,menuNodeId));
	}
	
	/**
	 * Check if menu tree node status is supported by connected camera
	 */
	public int menuNodeStatusIsSupported() {
		return (jni_menuNodeStatusIsSupported(m_ID));
	}
	
	/**
	 * Requests menu status for the given mode
	 * 
	 * @param menuNodeId		ID of the node
	 */
	public int menuNodeGetStatus(int menuNodeId) {
		return (jni_menuGetNodeStatus(m_ID, menuNodeId));	
	}
	
	/**
	 * Get current notification
	 */
	public int notificationGet() {
		return (jni_notificationGet(m_ID));
	}

	/**
	 * Notifies the API that the timeout associated with the current
	 * notification has expired and that the notification should be 
	 * closed.
	 * 
	 * @param notificationId	ID of the notification
	 */
	public int notificationTimeout(String notificationId) {
		return (jni_notificationTimeout(m_ID, notificationId));
	}
	
	/**
	 * Sends response for current notification.
	 * 
	 * @param notificationId	ID of the notification
	 * @param response			value of response selected
	 */
	public int notificationResponse(String notificationId, int response) {
		return (jni_notificationResponse(m_ID, notificationId, response));
	}
	
	/**
	 * Gets stats regarding camera connection
	 * 
	 * @param statsType		value of stats type selected
	 * 						<code>0</code> Tx Packets
	 * 						<code>1</code> Tx Bytes
	 * 						<code>2</code> Rx Packets
	 * 						<code>3</code> Rx Bytes
	 */
	public int getCameraConnectionStats(int statsType) {
		return (jni_getCameraConnectionStats(m_ID, statsType));
	}
	
	/**
	 * Requests RCP parameter status from the camera.
	 * 
	 * The status requested with this call will be returned in the callback: onStatusUpdate
	 * 
	 * @param parameterName		name of the RCP parameter (e.g. "RCP_PARAM_SLATE_SCENE")
	 */
	public int getstatus(String parameterName) {
		return (jni_getStatus(m_ID, parameterName));
	}
	
	/**
	 * Invoked when a formatted integer string is updated from the
	 * camera. The method is delegated to the parent listener.
	 * 
	 * @see RCPCameraListener#onUpdate
	 */
	@Override
	public void onUpdate(final String id, final String param) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onUpdate(id, param);
			}
		});		
	}
	
	/**
	 * Invoked when an integer value is updated from the
	 * camera. The method is delegated to the parent listener.
	 * 
	 * @see RCPCameraListener#onUpdateInt
	 */
	@Override
	public void onUpdateInt(final String id, final String param, final int value) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onUpdateInt(id, param, value);
			}
		});	
	}

	/**
	 * Invoked when an string parameter is updated from the
	 * camera. The method is delegated to the parent listener.
	 * 
	 * @see RCPCameraListener#onUpdateString
	 */
	@Override
	public void onUpdateString(final String id, final String param) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onUpdateString(id, param);
			}
		});	
	}

	/**
	 * Invoked when edit info of a string parameter is updated from the
	 * camera. The method is delegated to the parent listener.
	 * 
	 * @see RCPCameraListener#onUpdateStringEditInfo
	 */
	@Override
	public void onUpdateStringEditInfo(final String id, final String paramName, final int[] intParams, final String[] strParams) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onUpdateStringEditInfo(id, paramName, intParams, strParams);
			}
		});	
	}

	/**
	 * Invoked when a clip list is updated from the
	 * camera. The method is delegated to the parent listener.
	 * 
	 * @see RCPCameraListener#onUpdateClipList
	 */
	@Override
	public void onUpdateClipList(final String id, final int status, final String clipList) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onUpdateClipList(id, status, clipList);
			}
		});	
	}

	/**
	 * Invoked when edit info of a parameter is updated from the
	 * camera. The method is delegated to the parent listener.
	 * 
	 * @see RCPCameraListener#onUpdateEditInfo
	 */
	@Override
	public void onUpdateEditInfo(final String id, final String editInfo) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onUpdateEditInfo(id, editInfo);
			}
		});	
	}

	/**
	 * Invoked when a list is updated from the camera. This version uses arrays
	 * The method is delegated to the parent listener.
	 * 
	 * @see RCPCameraListener#onUpdateListStrings
	 */
    public void onUpdateListStrings(final String id, final String listHeader, final String listFlags,
    		final String[] listStrings, final int[] listValues) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onUpdateListStrings(id, listHeader, listFlags, listStrings, listValues);
			}
		});	
    }

	/**
	 * Invoked when the histogram is updated from the
	 * camera. The method is delegated to the parent listener.
	 * 
	 * @see RCPCameraListener#onUpdateHistogram
	 */
	@Override
	public void onUpdateHistogram(final String id, final int[] red, final int[] green, final int[] blue,
			final int[] luma, final int bottom_clip, final int top_clip, final String text) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onUpdateHistogram(id, red, green, blue, luma, bottom_clip, top_clip, text);
			}
		});	
	}
	
	/**
	 * Invoked when the connection error has occurred in the
	 * SDK. The method is delegated to the parent listener.
	 * 
	 * @see RCPCameraListener#onConnectionError
	 */
	@Override
	public void onConnectionError(final String id, final String msg) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onConnectionError(id, msg);
			}
		});		
	}

	/**
	 * Invoked when a state change is detected in the connection
	 * to the camera. The method is delegated to the parent listener.
	 * 
	 * @see RCPCameraListener#onStateChange
	 */
	@Override
	public void onStateChange(final String id, final String param) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onStateChange(id, param);
			}
		});	
	}
	
	/**
	 * Invoked when the status of a parameter is updated. The method 
	 * is delegated to the parent listener.
	 * 
	 * @see RCPCameraListener#onStatusUpdate
	 */
	@Override
	public void onStatusUpdate(final String id, final String param, final int isEnabled, final int isEnabledValid, final int isSupported, final int isSupportedValid) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onStatusUpdate(id, param, isEnabled, isEnabledValid, isSupported, isSupportedValid);
			}
		});	
	}
	
	/**
	 * Invoked when a camera notification is opened, updated, or closed.
	 * The method is delegated to the parent listener.
	 * 
	 * @see RCPCameraListener#onNotificationUpdate
	 */
	@Override
	public void onNotificationUpdate(final String id, final String nID, final String title, final String message, final String notificationInfo) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onNotificationUpdate(id, nID, title, message, notificationInfo);
			}
		});	
	}

	/**
	 * Invoked when audio input and output volumes are updated on the camera.
	 * The method is delegated to the parent listener.
	 * 
	 * @see RCPCameraListener#onAudioVuUpdate
	 */
	@Override
	public void onAudioVuUpdate(final String id, final String audioVu) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onAudioVuUpdate(id, audioVu);
			}
		});	
	}

	/**
	 * Invoked when menu node list is updated on the camera. The method
	 *  is delegated to the parent listener.
	 * 
	 * @see RCPCameraListener#onMenuNodeListUpdate
	 */
	@Override
    public void onMenuNodeListUpdate(final String id, final int nodeId, final String[] childList, final String[] ancestorList) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onMenuNodeListUpdate(id, nodeId, childList, ancestorList);
			}
		});	
	}
	
	/**
	 * Invoked when menu node status is updated.
	 * 
	 * @see RCPCameraListener#onMenuNodeStatusUpdate
	 */
    public void onMenuNodeStatusUpdate(final String id, final int nodeId, final int isEnabled, final int isEnabledValid, final int isSupported, final int isSupportedValid) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onMenuNodeStatusUpdate(id, nodeId, isEnabled, isEnabledValid, isSupported, isSupportedValid);
			}
		});	
    }

	/**
	 * Invoked when tagging information is updated on the camera.
	 * The method is delegated to the parent listener.
	 * 
	 * @see RCPCameraListener#onUpdateTag
	 */
	@Override
	public void onUpdateTag(final String id, final String param) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onUpdateTag(id, param);
			}
		});	
	}
	
	@Override
	public void onCameraInfo(final String id, final String camInfo) {
		m_Handler.post(new Runnable() {
			public void run() {
				System.out.println("RCPCamera is calling onCameraInfo");
				// Add the type: IP_Address or Serial
				m_DelegateListener.onCameraInfo(id, camInfo + ";" + m_IP);
			}
		});		
	}
	
	@Override
    public void onRftpStatusUpdate(final String id, final byte[] uuid, final int rftpType, final int rftpError, final int percentComplete, final String[] dirList) {
		m_Handler.post(new Runnable() {
			public void run() {
				m_DelegateListener.onRftpStatusUpdate(id, uuid, rftpType, rftpError, percentComplete, dirList);
			}
		});		
	}
	
	/** 
	 * Check if parameter is supported by connected camera.
	 * 
	 * @param parameterName		name of the RCP parameter (e.g. "RCP_PARAM_SATURATION")
	 * 
	 * @return boolean value of whether the RCP parameter is supported by the connected camera
	 */
	public int getIsSupported(String parameterName) {
		return jni_getIsSupported(m_ID, parameterName);
	}	
	
	/** 
	 * Check if parameter property of a parameter is supported by connected camera.
	 * 
	 * @param parameterName		name of the RCP parameter (e.g. "RCP_PARAM_SATURATION")
	 * @param propertyType		value of property type to be checked
	 * 							<code>0</code> get
	 * 							<code>1</code> get list
	 * 							<code>2</code> get status
	 * 							<code>3</code> send
	 * 							<code>4</code> set int
	 * 							<code>5</code> set uint
	 * 							<code>6</code> set str
	 * 							<code>7</code> set list
	 * 							<code>8</code> edit info
	 * 							<code>9</code> update list only on close
	 * 
	 * @return boolean value of whether the RCP parameter is supported by the connected camera
	 */
	public int getPropertyIsSupported(String parameterName, int propertyType) {
		return jni_getPropertyIsSupported(m_ID, parameterName, propertyType);
	}
	
	/**
	 * Sets an unsigned integer parameter on the camera.
	 * 
	 * @param parameterName		name of the RCP parameter (e.g. "RCP_PARAM_SATURATION")
	 * @param value				value to set (e.g. 1200)
	 */
	public void setParameterUnsigned(String parameterName, long value) {
		jni_setParameterUnsigned(m_ID, parameterName, (long)value);
	}
	
	public String getAPIVersion() {
		return jni_getAPIVersion();
	}
	
	public String getParameterName (int parameterId) {
		return jni_getParameterName(m_ID, parameterId);
	}

	/**
	 * Enable or disable periodic parameter updates
	 *
	 * @param parameterName		name of the RCP parameter (e.g. "RCP_PARAM_HISTOGRAM")
	 * @param enable			true to enable, false to disable
	 *
	 * @return boolean value of whether the RCP parameter is supported by the connected camera
	 */
	public void setPeriodicEnable(String parameterName, boolean enable) {
		jni_setPeriodicEnable(m_ID, parameterName, enable);
	}

	public int rftpGetIsSupported() {
		return jni_rftpIsSupported(m_ID);
	}

	public int rftpSendFile(String localFile, String cameraFile, boolean compress, byte[] uuid_out) {
		int ret = -1;
		if (!localFile.isEmpty() && !cameraFile.isEmpty() && (uuid_out.length == RCP_UUID_LEN)) {
			ret = jni_rftpSendFile(m_ID, localFile, cameraFile, compress, uuid_out);
		}
		return ret;
	}
	
	public int rftpRetrieveFile(String localFile, String cameraFile, int maxFileSize, boolean compressionAllowed, byte[] uuid_out) {
		int ret = -1;
		if (!localFile.isEmpty() && !cameraFile.isEmpty() && (uuid_out.length == RCP_UUID_LEN)) {
			ret = jni_rftpRetrieveFile(m_ID, localFile, cameraFile, maxFileSize, compressionAllowed, uuid_out);
		}
		return ret;
	}

	public int rftpAbortTransfer(byte[] uuid_in) {
		int ret = -1;
		if (uuid_in.length == RCP_UUID_LEN) {
			ret = jni_rftpAbortTransfer(m_ID, uuid_in);
		}
		return ret;
	}

	public int rftpDeleteFile(String cameraFile, byte[] uuid_out) {
		int ret = -1;
		if (!cameraFile.isEmpty()) {
			ret = jni_rftpDeleteFile(m_ID, cameraFile, uuid_out);
		}
		return ret;
	}
	
	public int rftpDirectoryList(String path, byte[] uuid_out) {
		int ret = -1;
		if (!path.isEmpty() && (uuid_out.length == RCP_UUID_LEN)) {
			ret = jni_rftpDirList(m_ID, path, uuid_out);
		}
		return ret;
	}
}
