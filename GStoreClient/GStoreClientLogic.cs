﻿using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public delegate void DelAddMsg(string s);

namespace GStoreClient
{

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

    public class GStoreClient
    {
        Queue<String> commandQueue = new Queue<String>();

        private static int CACHE_LIMIT = 20;
        private GStoreServerService.GStoreServerServiceClient current_server;
        private String current_server_id;
        private string username;
        private string hostname;
        // dictionary with serverID as key and clientStruct
        private Dictionary<string, ClientStruct> serverMap =
            new Dictionary<string, ClientStruct>();
        // dictionary with partitionID as key and list of serverID's
        private Dictionary<string, List<string>> partitionMap = new Dictionary<string, List<string>>();

        private ResponseCache responseCache = new ResponseCache(CACHE_LIMIT);

        private Dictionary<Tuple<String, String>, Tuple<int, int>> timestamp = new Dictionary<Tuple<String, String>, Tuple<int, int>>();


        public GStoreClient(String user, String host, String args)
        {
            username = user;
            hostname = host;

            //partitions come in command line format: -p partition_id partition_master_id partition_master_url other_servers_id other_servers_url -p 

            String[] partitions = args.Split("-p ", StringSplitOptions.RemoveEmptyEntries);

            foreach (var partition in partitions)
            {
                AddPartitionToDict(partition);
            }
        }

        private void AddServerToDict(String server_id, String url)
        {

            GrpcChannel channel = GrpcChannel.ForAddress("http://" + url);
            GStoreServerService.GStoreServerServiceClient client = new GStoreServerService.GStoreServerServiceClient(channel);
            ClientStruct server = new ClientStruct(url, client);
            serverMap.Add(server_id, server);
        }

        // receives arguments in the format: partition_master_id partition_master_url server2_id server2_url ....
        private void AddPartitionToDict(String servers)
        {
            String[] fields = servers.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            String partition_id = fields[0];

            partitionMap.Add(partition_id, new List<string>());
            for (int i = 1; i < fields.Length; i += 2)
            {
                String server_id = fields[i];
                String server_url = fields[i + 1];

                partitionMap[partition_id].Add(server_id);
                if (!serverMap.ContainsKey(server_id))
                    AddServerToDict(server_id, server_url);
            }
        }

        public void ReadValue(
   string partition_id, string object_id, string server_id)
        {
            int initialCount = this.partitionMap[partition_id].Count;
            List<String> serversLeft = new List<String>(this.partitionMap[partition_id]);
            bool success = false;


            if (!partitionMap.ContainsKey(partition_id))
            {
                Console.Error.WriteLine("Error: Partition " + partition_id + " does not exist in the system");
                return;
            }

            if (current_server == null || !this.partitionMap[partition_id].Contains(current_server_id))
            {
                // when there is no current server attached and the server_id argument is -1
                // it will try to connect to the partition master
                if (this.partitionMap[partition_id].Contains(server_id))
                {
                    current_server_id = server_id;
                    current_server = serverMap[server_id].service;

                }
                else
                {

                    current_server_id = this.partitionMap[partition_id].First();
                    current_server = serverMap[this.partitionMap[partition_id].First()].service;
                }
            }

            while (!success)
            {

                Console.WriteLine("Connecting to server " + current_server_id + "...");

                if (!partitionMap.ContainsKey(partition_id))
                {
                    Console.Error.WriteLine("Error: Partition " + partition_id + " does not exist in the system");
                    return;
                }

                try
                {
                    serversLeft.Remove(current_server_id);
                    ReadValueReply reply = current_server.ReadValue(new ReadValueRequest
                    {
                        PartitionId = partition_id,
                        ObjectId = object_id,
                    }, deadline: DateTime.UtcNow.AddSeconds(5));


                    if (reply.Value.Equals("N/A"))
                    {
                        Console.Error.WriteLine("Error: Unable to fetch object " + object_id + " in server " + current_server_id + ". Relocating...");
                    }
                    else
                    {
                        String value = reply.Value;
                        Tuple<String, String> part_obj_tup = new Tuple<String, String>(partition_id, object_id);
                        if (!timestamp.ContainsKey(part_obj_tup)) {
                            timestamp.Add(part_obj_tup, new Tuple<int, int>(reply.ServerIndex, reply.Version));
                            responseCache.addEntry(new Tuple<string, string, string>(partition_id, object_id, value));
                        }
                        else {
                            int serverIndex = timestamp[part_obj_tup].Item1;
                            int version = timestamp[part_obj_tup].Item2;
                            //Server outdated get cache value
                            if (serverIndex>reply.ServerIndex || (serverIndex==reply.ServerIndex && version > reply.Version)) {
                                Console.WriteLine("Server outdated, reading from cache!");
                                value = responseCache.getCorrectValue(new Tuple<string, string>(partition_id, object_id));
                                //if cache has entry
                                if(value == null)
                                    value = reply.Value;
                            }
                            else {
                                responseCache.addEntry(new Tuple<string, string, string>(partition_id, object_id, value));
                                timestamp[part_obj_tup] = new Tuple<int, int>(reply.ServerIndex, reply.Version);
                            }
                        }
                        Console.WriteLine("Read value " + value + " on partition " + partition_id + " on object " + object_id);
                        return;
                    }

                }
                catch (RpcException)
                {

                    Console.Error.WriteLine("Error: Connection failed to server " + current_server_id + " of partition " + partition_id);


                }

                if (serversLeft.Contains(server_id) && this.partitionMap[partition_id].Contains(server_id))
                {

                    current_server_id = server_id;
                    current_server = this.serverMap[server_id].service;
                }
                else
                {

                    if (serversLeft.Count != 0)
                    {
                        current_server_id = serversLeft.First();
                        current_server = this.serverMap[current_server_id].service;
                    }
                    else
                    {
                        Console.WriteLine("Could not find object " + object_id + " in partition " + partition_id + ". Aborting...");
                        return;
                    }

                }
            }
        }


        public bool WriteValue(
               string partition_id, string object_id, string value)
        {
            bool success = false;
            int current_server_index = 0;
            while (!success && partitionMap[partition_id].Count() != 0 && partitionMap[partition_id].Count() > current_server_index)
            {
                //Assuming the replica master is the first element of the list  
                string server_id = partitionMap[partition_id][current_server_index];

                GStoreServerService.GStoreServerServiceClient master = serverMap[server_id].service;
                current_server = master;
                current_server_id = server_id;
                Console.WriteLine("Connecting to master replica with server_id " + server_id + " of partition " + partition_id);
                Console.WriteLine("Sending Write operation to partition " + partition_id + " on object " + object_id + " with value '" + value + "'");

                try
                {
                    WriteValueReply reply = master.WriteValue(new WriteValueRequest
                    {
                        PartitionId = partition_id,
                        ObjectId = object_id,
                        Value = value
                    }, deadline: DateTime.UtcNow.AddSeconds(5));
                    //If the reply is false the requested server is not the leader
                    if (reply.Ok == false) {
                        Console.WriteLine("Write failed on server " + server_id + " because he is not the leader");
                        //remove every server until reply.CurrentLeader
                        removeServers(partition_id, reply.CurrentLeader);
                        current_server_index = 0;
                    } else { // otherwise the server was the correct leader and the write was successful
                        Console.WriteLine("Write successful on server " + server_id);
                        //remove every server until reply.CurrentLeader
                        removeServers(partition_id, server_id);
                        success = true;
                        return reply.Ok;
                    }
                    Console.WriteLine("Retrying write..");

                }
                catch (RpcException e)
                {
                    Console.Error.WriteLine("Error: Connection failed to server " + server_id + " of partition " + partition_id);
                    current_server_index++;

                }
            }
            return false;

        }

        public void ListServer(String server_id)
        {
            if (!serverMap.ContainsKey(server_id))
            {
                Console.Error.WriteLine("Error: Unable to locate server " + server_id);
                return;
            }
            GStoreServerService.GStoreServerServiceClient server = serverMap[server_id].service;

            try
            {
                ListServerObjectsReply reply = server.ListServerObjects(new ListServerObjectsRequest { }, deadline: DateTime.UtcNow.AddSeconds(5));

                Console.WriteLine("Server " + server_id + " stores the following objects:");


                foreach (var obj in reply.Objects)
                {

                    if (obj.IsMaster)
                        Console.WriteLine("-> Object " + obj.ObjectId + " with value '" + obj.Value + "' in partition " + obj.PartitionId + "(master for this partition).");
                    else
                        Console.WriteLine("-> Object " + obj.ObjectId + " with value " + obj.Value + " in partition " + obj.PartitionId);
                }
            }
            catch (RpcException)
            {
                Console.Error.WriteLine("Error: Connection failed to server " + server_id);

            }
        }

        public void ListGlobal()
        {
            List<String> masters = new List<String>();
            foreach (var pair in serverMap)
            {
                Console.WriteLine("Listglobal server " + pair.Key);
                ListServer(pair.Key);
                Console.WriteLine();

                
            }
            Console.WriteLine("List Global completed");
        }

        public void readScriptFile(String file)
        {

            String line;
            if (!File.Exists(file))
            {
                Console.Error.WriteLine("Error: Script file " + file + " not found");
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

        public bool isEnd(String command)
        {
            return command.Split(" ")[0] == "end-repeat";
        }

        public bool isBegin(String command)
        {
            return command.Split(" ")[0] == "begin-repeat";
        }
        public void beginRepeat(int x, int line)
        {
            List<String> block = new List<String>();

            //Repeating x times
            for (int i = 1; i <= x; i++)
            {
                int current_line = 1;
                int executed_commands = 1;
                int context = 0;

                //
                foreach (var command in commandQueue)
                {
                    if (current_line > line && isEnd(command))
                    {
                        //Reached current context end-repeat
                        if (context == 0)
                            break;

                        context--;  //Change context
                        executed_commands++;
                    }
                    //Only exec commands that come after called begin and on current context
                    if (current_line > line && context == 0)
                    {
                        //$i --> current iteration
                        String rcommand = command.Replace("$i", i.ToString());

                        //Change context
                        if (isBegin(command))
                            context++;

                        runOperation(rcommand, line + executed_commands);
                        executed_commands++;
                    }
                    current_line++;
                }
            }

        }


        public int updateContext(String command)
        {
            if (isEnd(command))
                return -1;
            else if (isBegin(command))
                return 1;
            else
                return 0;
        }

        public void removeServers(String partition, String server_id)
        {

            String remove_id = "";
            while (this.partitionMap[partition].Count != 0) {
                remove_id = this.partitionMap[partition].First();
                if (remove_id == server_id) break; 
                this.partitionMap[partition].Remove(this.partitionMap[partition][this.partitionMap[partition].IndexOf(remove_id)]);

                Console.Error.WriteLine("Removing server  " + remove_id);
            }
            
            if (this.partitionMap[partition].Count == 0) 
                Console.Error.WriteLine("Error: No more servers available in partition");
        }

        public bool isCorrectRepeat()
        {
            int begin = 0, end = 0;
            foreach (var command in commandQueue)
            {
                if (isBegin(command)) begin++;

                else if (isEnd(command)) end++;

                if (end > begin) return false;
            }

            return begin == end;
        }
        public void processCommands()
        {
            int line = 1;
            int context = 0;

            if (!isCorrectRepeat())
            {
                Console.Error.WriteLine("Syntax Error: Begin/End-repeat loop");
                commandQueue.Clear();
            }
            foreach (var command in commandQueue)
            {
                if (context == 0)
                    runOperation(command, line);
                context += updateContext(command);
                line++;
            }
            commandQueue.Clear();
        }

        public void runOperation(string op, int line)
        {
            String partition_id, object_id, server_id;
            string[] args = op.Split(" ");
            Console.WriteLine("");
            switch (args[0])
            {
                case "read":
                    partition_id = args[1];
                    object_id = args[2];
                    server_id = args[3];
                    Console.WriteLine("Read request received");
                    ReadValue(partition_id, object_id, server_id);
                    break;
                case "write":
                    partition_id = args[1];
                    object_id = args[2];
                    String value = op.Split('"')[1];
                    Console.WriteLine("Write request received");
                    bool done = WriteValue(partition_id, object_id, value);
                    if (done) Console.WriteLine("Value written!");
                    else Console.Error.WriteLine("Error: Could not write given value");
                    break;
                case "listServer":
                    server_id = args[1];
                    Console.WriteLine("List Server request received");
                    Console.WriteLine("Reading all objects from server " + server_id);
                    ListServer(server_id);
                    break;
                case "listGlobal":
                    Console.WriteLine("List Global request received");
                    Console.WriteLine("Reading all objects from the system");
                    ListGlobal();
                    break;
                case "wait":
                    String ms = args[1];
                    Console.WriteLine("Wait request received");
                    Console.WriteLine("Delaying execution for " + ms + " milliseconds"); 
                    System.Threading.Thread.Sleep(int.Parse(ms));
                    Console.WriteLine("Program resumed");
                    break;
                case "begin-repeat":
                    beginRepeat(int.Parse(args[1]), line);
                    break;
                case "end-repeat":
                    break;
                default:
                    Console.Error.WriteLine("Error: Not a recognized operation");
                    break;
            }
        }

        public void addComand(String command)
        {
            commandQueue.Enqueue(command);
            System.Diagnostics.Debug.WriteLine("added command:", command);
        }

        public void ServerShutdown(Server server)
        {
            server.ShutdownAsync().Wait();
        }
    }
}
