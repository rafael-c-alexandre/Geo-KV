﻿using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public delegate void DelAddMsg(string s);

namespace GStoreClient {

    struct ClientStruct
    {
        public string url;
        public GStoreServerService.GStoreServerServiceClient service;

        public ClientStruct(string u, GStoreServerService.GStoreServerServiceClient s)
        {
            url = u;
            service = s;
        }
    }
    public interface IGStoreClientService {
        bool AddMsgtoGUI(string s);
    }
    public class GStoreClient : IGStoreClientService {
        private  GrpcChannel channel;
        Queue<String> commandQueue = new Queue<String>();
        private GStoreServerService.GStoreServerServiceClient current_server;
        private string username;
        private string hostname;
        // dictionary with serverID as key and clientStruct
        private Dictionary<string, ClientStruct> serverMap =
            new Dictionary<string,ClientStruct>();
        // dictionary with partitionID as key and list of serverID's
        private Dictionary<string, List<string>> partitionMap = new Dictionary<string, List<string>>();

        public GStoreClient(String user, String host ) {
            username = user;
            hostname = host;
        }

        public bool AddMsgtoGUI(string s) {
            return true;
        }

        private void addServerToDict(String server_id, String url)
        {
            GrpcChannel channel = GrpcChannel.ForAddress("http://" + url);
            GStoreServerService.GStoreServerServiceClient client = new GStoreServerService.GStoreServerServiceClient(channel);
            ClientStruct server = new ClientStruct(url, client);
            serverMap.Add(server_id, server);
        }


        private void  addPartitionToDict(String id, List<string> servers)
        {
            partitionMap.Add(id, servers);
        }


        public String ReadValue(
           string partitionId, string objectId, string serverId)
        {
            AppContext.SetSwitch(
                      "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            ReadValueReply reply = current_server.ReadValue(new ReadValueRequest
            {
                PartitionId = partitionId,
                ObjectId = objectId,
            });
            if (reply.Value.Equals("N/A"))
            {
                AppContext.SetSwitch(
                        "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                GStoreServerService.GStoreServerServiceClient new_server = serverMap[serverId].service;

                reply = new_server.ReadValue(new ReadValueRequest
                {
                    PartitionId = partitionId,
                    ObjectId = objectId,
                });
                current_server = new_server;
            }
            return reply.Value;
        }

        public bool WriteValue(
           string partitionId, string objectId, string value)
        {

            AppContext.SetSwitch(
                    "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            //Assuming the replica master is the first element of the list - 
            string serverID = partitionMap[partitionId].ElementAt(0);

            GStoreServerService.GStoreServerServiceClient client = serverMap[serverID].service;

            WriteValueReply reply = client.WriteValue(new WriteValueRequest
            {
                PartitionId = partitionId,
                ObjectId = objectId,
                Value = value
            });

            return reply.Ok;
        }

        public void readScriptFile(String file)
        {
            String line;
            if (!File.Exists(file))
            {


                //TODO
            }
            else
            {
                System.IO.StreamReader fileStream = new System.IO.StreamReader(file);

                while ((line = fileStream.ReadLine()) != null)
                {
                    addComand(line);
                }

                fileStream.Close();

                processCommands();
            }
        }

        public void processCommands()
        {
            foreach (var command in commandQueue)
            {
                runOperation(command);
                commandQueue.Dequeue();
            }

        }

        public void runOperation(string op)
        {
            string[] args = op.Split(" ");
            switch (args[0])
            {
                case "read":
                    String partition_id = args[1];
                    String object_id = args[2];
                    String server_id = args[3];
                    ReadValue(partition_id, object_id, server_id);
                    break;
                case "write":
                    break;
                case "ListServer":
                    break;
                case "ListGlobal":
                    break;
                case "wait":
                    break;
                case "begin-repeat":
                    break;
                case "end-repeat":
                    break;
                default:
                    break;
            }
        }

        public void addComand(String command)
        {
            commandQueue.Enqueue(command);
            System.Diagnostics.Debug.WriteLine("added command:", command);
        }

        public void ServerShutdown(Server server) {
            server.ShutdownAsync().Wait();
        }
    }


    public class ClientService : GStoreClientService.GStoreClientServiceBase
    {
        IGStoreClientService clientLogic;
        

        public ClientService(IGStoreClientService clientLogic)
        {
            this.clientLogic = clientLogic;
        }

        public override Task<RecvMsgReply> RecvMsg(
            RecvMsgRequest request, ServerCallContext context)
        {
            return Task.FromResult(UpdateGUIwithMsg(request));
        }

        public RecvMsgReply UpdateGUIwithMsg(RecvMsgRequest request)
        {
            if (clientLogic.AddMsgtoGUI(request.Msg))
            {
                return new RecvMsgReply
                {
                    Ok = true
                };
            }
            else
            {
                return new RecvMsgReply
                {
                    Ok = false
                };

            }
        }

    }
}
