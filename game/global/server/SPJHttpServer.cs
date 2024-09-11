using Godot;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Linq;

class MalformedHttpException : System.Exception
{
	public MalformedHttpException(string message) : base() { }
};

class HttpPacket
{
	public Dictionary<string, string> headers = new();
	public static string NL = "\r\n";
}

class HttpRequest : HttpPacket
{
	public string method = "";
	public string path = "";
	public string version = "";
	public string body = "";

	public HttpRequest(string http_packet)
	{
		var phase = 0;
		var pointer = 0;
		do
		{
			var result = http_packet.IndexOf(NL, pointer);
			string examined;
			if (result == -1)
			{
				examined = http_packet.Substring(pointer);
				pointer = result;
			}
			else
			{
				examined = http_packet.Substring(pointer, result - pointer);
				pointer = result + NL.Length;
			}

			if (phase == 0)
			{ // HTTP Request type
				var http_start = examined.Split(" ", 3);
				if (http_start.Length != 3) throw new MalformedHttpException("length is not 3");
				method = http_start[0];
				path = http_start[1];
				version = http_start[2];
				phase = 1;
			}
			else if (phase == 1 && examined != "")
			{
				var headers = examined.Split(": ", 2);
				if (headers.Length != 2) throw new MalformedHttpException("length is not 2");
				this.headers.Add(headers[0], headers[1]);
			}
			else if (phase == 1)
			{
				phase = 2;
			}
			else
			{
			}
		} while (pointer != -1 && phase != 2);
		if (phase == 2)
		{
			body = http_packet.Substring(pointer);
		}
	}
}

class HttpResponse : HttpPacket
{
	public string version;
	public string status;

	public string Serialize()
	{
		var packet = $"{version} {status}{NL}";

		foreach (var header in headers)
		{
			packet += $"{header.Key}: {header.Value}{NL}";
		}
		packet += NL;
		return packet;
	}
}

[GlobalClass]
public partial class SPJHttpServer : Node
{
	private TcpServer server = new();
	private List<StreamPeerTcp> clients = new();

	public override void _Ready()
	{
		base._Ready();
		SPJ.HttpServer = this;
	}

	public void Start()
	{
		SPJ.RunServer(server, 12003);
	}

	public int GetPort()
	{
		return server.GetLocalPort();
	}

	public override void _Process(double delta)
	{
		while (server.IsConnectionAvailable())
		{
			var connection = server.TakeConnection();
			clients.Add(connection);
		}

		foreach (var client in clients)
		{
			client.Poll();
			var status = client.GetStatus();
			if (status == StreamPeerTcp.Status.Connected)
			{
				var bytes = client.GetAvailableBytes();
				if (bytes > 0)
				{
					HandleHttp(client, client.GetUtf8String(bytes));
					client.DisconnectFromHost();
				}
			}
		}
		var removed = clients.RemoveAll(client => client.GetStatus() == StreamPeerTcp.Status.None);
	}

	private byte[] GetFile(HttpRequest request, ref HttpResponse response)
	{
		var path = request.path;
		if (path.EndsWith("/"))
		{
			path += "index.html";
		}
		var ext = path.Split("/").Last().Split(".").Last();
		string mimetype;
		if (ext == "html")
		{
			mimetype = "text/html";
		}
		else if (ext == "js")
		{
			mimetype = "text/javascript";
		}
		else if (ext == "css")
		{
			mimetype = "text/css";
		}
		else if (ext == "png")
		{
			mimetype = "text/png";
		}
		else if (ext == "jpg")
		{
			mimetype = "text/jpeg";
		}
		else if (ext == "svg")
		{
			mimetype = "image/svg+xml";
		}
		else
		{
			mimetype = "application/octet-stream";
		}

		var bytes = FileAccess.GetFileAsBytes("res://assets/controller" + path);
		var e = FileAccess.GetOpenError();
		if (e != Error.Ok)
		{
			response.status = "404 Not Found";
		}
		else
		{
			response.status = "200 OK";
			response.headers.Add("Content-Type", mimetype);
		}
		return bytes;

	}
	private void HandleHttp(StreamPeerTcp client, string http_packet)
	{
		var request = new HttpRequest(http_packet);
		var response = new HttpResponse();
		byte[] content = { };
		response.version = "HTTP/1.1";
		if (request.method != "GET")
		{
			response.status = "405 Method Not Allowed";
		}
		else if (request.path == "/ws")
		{
			response.status = "200 OK";
			response.headers.Add("Content-Type", "text/plain");
			content = SPJ.GameServer.GetPort().ToString().ToUtf8Buffer();
		}
		else
		{
			content = GetFile(request, ref response);
		}
		response.headers.Add("Content-Length", content.Length.ToString());
		response.headers.Add("Access-Control-Allow-Origin", "*");

		var s = response.Serialize();
		client.PutData(s.ToUtf8Buffer());
		client.PutData(content);
	}
}