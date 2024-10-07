﻿using System.Net;
using NettyClient;

#if DEBUG
const int clientCount = 1;
var endpoint = new IPEndPoint(IPAddress.Loopback, 20000);
#else
const int clientCount = 1000;
var endpoint = new IPEndPoint(IPAddress.Loopback, 20000);
// var endpoint = new IPEndPoint(IPAddress.Parse("10.70.210.12"), 20000);
#endif

var options = new ParallelOptions { MaxDegreeOfParallelism = clientCount };
await Parallel.ForAsync(0, clientCount, options, (_, cancel) => new Client(endpoint).RunAsync(cancel));