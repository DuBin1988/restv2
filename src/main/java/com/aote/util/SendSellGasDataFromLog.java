package com.aote.util;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStream;
import java.io.Writer;

import org.apache.http.HttpResponse;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.entity.StringEntity;
import org.apache.http.impl.client.DefaultHttpClient;
import org.codehaus.jettison.json.JSONArray;
import org.codehaus.jettison.json.JSONException;
import org.codehaus.jettison.json.JSONObject;

import sun.net.www.http.HttpClient;

public class SendSellGasDataFromLog {

	// ��̨�����ַ
	private static String restIp = "http://127.0.0.1/rs/db/save";

	// ��Ҫ�����ʵ�������
	private static String entityName = "t_sellinggas";

	public static void main(String[] args) {
		try {

			// �����־
			FileWriter outwr = new FileWriter("c://output.txt");
			BufferedWriter bw = new BufferedWriter(outwr);
			
			FileWriter ew = new FileWriter("c://error.txt");
			BufferedWriter errwr = new BufferedWriter(ew);
		
			// ����趨Ŀ¼��ÿ���ļ�
			// ����0ΪĿ¼������Ŀ¼�е������ļ�����ѭ������, ���ݹؼ����ж�
			String fold = "c:\\logdatas";
			String key = "\"entity\":\"t_sellinggas\"";
			File dir = new File(fold);
			for (File file : dir.listFiles()) {
				System.out.println("���ڴ���" + file.getName());
				processOneFile(file, key, bw,errwr);
			}
			bw.flush();
			bw.close();
			errwr.flush();
			errwr.close();
		} catch (Exception e) {
			e.printStackTrace();
			return;
		}
		System.out.println("�������");
	}

	/**
	 * �������ļ� ���ݹؼ����ҵ����� 1)�������󵽺�̨�������ݱ��� 2)д����־�ļ�
	 */
	private static void processOneFile(File logFile, String keys,
			BufferedWriter bw,BufferedWriter errwr) {
		{
			try {
				// ����־�ļ��Լ�����ļ�
				FileReader reader = new FileReader(logFile);
				BufferedReader br = new BufferedReader(reader);
				// ����־�ļ����һ��
				int i = 0;
				String str = br.readLine();
				while (str != null) {
					// ��������ؼ��֣�����һ�и�ʽ����������ļ���
					if (contains(str, keys)) {
						bw.append(i + "--" + str);
						bw.newLine();
						String temp = str;
						temp = temp.substring(temp.indexOf("["), temp.length());
						// ����������
						processOneData(temp,errwr);
					}
					str = br.readLine();
					i++;
				}
				br.close();
				reader.close();
			} catch (Exception e) {
				System.out.println(e.getMessage());
			}
		}
	}

	// �ж�source���Ƿ��йؼ���keys���ؼ��ְ����ŷָ���source�а���
	// ���������йؼ��֣������档���򣬷���false��
	public static boolean contains(String source, String keys) {
		for (String key : keys.split(",")) {
			// ֻҪ�в������ģ��ͷ���false
			if (!source.contains(key)) {
				return false;
			}
		}
		return true;
	}

	// �������ݴ���
	// ѭ��JSON����,ָ��ʵ��Ķ��������̨���ʹ洢����
	private static void processOneData(String str,BufferedWriter errwr) {
		try {
			JSONArray objects = new JSONArray(str);
			for (int i = 0; i < objects.length(); i++) {
				JSONObject obj = (JSONObject) objects.get(i);
				if (!obj.has("entity")) {
				 	 continue;
				}
				String entity = obj.get("entity").toString();
				// ���ʵ��������ͬ�����ͱ�������
				if (entity.equals(entityName)) {
					DefaultHttpClient client = new DefaultHttpClient();
					HttpPost httpPost = new HttpPost(restIp);
					StringEntity strEntity = new StringEntity(obj.toString(),
							"utf-8");
					httpPost.setEntity(strEntity);
					HttpResponse httpResponse = client.execute(httpPost);
				}
			}
		} catch (Exception e) {
			System.out.println(e.getMessage());
		}
	}
}
