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

	// 后台服务地址
	private static String restIp = "http://127.0.0.1/rs/db/save";

	// 需要保存的实体对象名
	private static String entityName = "t_sellinggas";

	public static void main(String[] args) {
		try {

			// 输出日志
			FileWriter outwr = new FileWriter("c://output.txt");
			BufferedWriter bw = new BufferedWriter(outwr);
			
			FileWriter ew = new FileWriter("c://error.txt");
			BufferedWriter errwr = new BufferedWriter(ew);
		
			// 检测设定目录中每个文件
			// 参数0为目录名，对目录中的所有文件进行循环处理, 根据关键字判断
			String fold = "c:\\logdatas";
			String key = "\"entity\":\"t_sellinggas\"";
			File dir = new File(fold);
			for (File file : dir.listFiles()) {
				System.out.println("正在处理：" + file.getName());
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
		System.out.println("处理完成");
	}

	/**
	 * 处理单个文件 根据关键字找到数据 1)发送请求到后台进行数据保存 2)写入日志文件
	 */
	private static void processOneFile(File logFile, String keys,
			BufferedWriter bw,BufferedWriter errwr) {
		{
			try {
				// 打开日志文件以及结果文件
				FileReader reader = new FileReader(logFile);
				BufferedReader br = new BufferedReader(reader);
				// 从日志文件里读一行
				int i = 0;
				String str = br.readLine();
				while (str != null) {
					// 如果包括关键字，对这一行格式化后存入结果文件中
					if (contains(str, keys)) {
						bw.append(i + "--" + str);
						bw.newLine();
						String temp = str;
						temp = temp.substring(temp.indexOf("["), temp.length());
						// 处理单条数据
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

	// 判断source中是否含有关键字keys，关键字按逗号分割。如果source中包含
	// 给定的所有关键字，返回真。否则，返回false。
	public static boolean contains(String source, String keys) {
		for (String key : keys.split(",")) {
			// 只要有不包含的，就返回false
			if (!source.contains(key)) {
				return false;
			}
		}
		return true;
	}

	// 单条数据处理
	// 循环JSON对象,指定实体的对象才往后台发送存储请求
	private static void processOneData(String str,BufferedWriter errwr) {
		try {
			JSONArray objects = new JSONArray(str);
			for (int i = 0; i < objects.length(); i++) {
				JSONObject obj = (JSONObject) objects.get(i);
				if (!obj.has("entity")) {
				 	 continue;
				}
				String entity = obj.get("entity").toString();
				// 如果实体类型相同，发送保存请求
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
