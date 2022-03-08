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

import java.io.DataOutputStream;
import java.io.DataInputStream;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.Socket;
import java.net.SocketException;
import java.net.InetSocketAddress;
import java.io.*;

public class NetworkComm implements RCPCommInterface {

	private final static int RCP_PORT = 1111;
	
	private Socket m_listenerSocket;
	private DataOutputStream m_outStream;
	private DataInputStream m_inStream;
	
	private String m_IP = null;
	private boolean m_openStatus = false;
	
	public NetworkComm() {
		
	}
	
	@Override
	public int open(String ip) {

		m_IP = ip;
		int status = 0;
		
		try { 
			m_listenerSocket = new Socket();	
			m_listenerSocket.connect(new InetSocketAddress(m_IP, RCP_PORT), 5000);
			m_inStream = new DataInputStream(m_listenerSocket.getInputStream());
			m_outStream = new DataOutputStream(m_listenerSocket.getOutputStream());
			m_openStatus = true;
		} catch (IOException e) {
			m_openStatus = false;
			status = -1;
		}
		
		return status;
	}

	@Override
	public void close() {

		try {
			m_listenerSocket.close();
		} catch (IOException e) {
		}
		m_openStatus = false;
	}

	@Override
	public int read(byte[] buffer, int len) {
		int nbytes = 0;
		
		try {
			nbytes = m_inStream.read(buffer);		
		} catch (IOException e) {
		}

		return nbytes;
	}

	@Override
	public int write(byte[] buffer) {
		
		try {
			m_outStream.write(buffer);
		} catch (IOException e) {
		}
		
		return 0;
	}

	@Override
	public int detect() {
		// TODO Auto-generated method stub
		return 0;
	}

}
