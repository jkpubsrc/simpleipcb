#!/usr/bin/python3
# -*- coding: utf-8 -*-


import timeit

import jk_simpleipcb



binFilePath = "../../SimpleIPCB/ExampleUTF8Echo/bin/Release/ExampleUTF8Echo.exe"

b = jk_simpleipcb.SimpleInterProcessCommunicationBridge()
b.launchComponentProcess("MyFancyComponent", binFilePath, "str", "str")

NUMBER_OF_REPEATS = 100000


start = timeit.time.time()
for i in range(0, NUMBER_OF_REPEATS):
	b.send("MyFancyComponent", "echo-test", "The quick brown fox jumps over the lazy dog.")
end = timeit.time.time()

totalDuration = end - start
print("Total duration for " + str(NUMBER_OF_REPEATS) + " message exchanges: " + str(totalDuration))
durationperRequest = totalDuration/NUMBER_OF_REPEATS
print("Total duration of a single message exchange: " + str(durationperRequest*1000) + " ms")
numberOfRequestsPerSecond = 1/durationperRequest
print("Number of requests possible per second: " + str(int(numberOfRequestsPerSecond)))



