import http.client
import json
import os
import xml.etree.ElementTree as ET

# responsekey file name
responsekey_filename = "responseid.txt"
lastresponseid = "0"

# get key from file
if os.path.exists(responsekey_filename):
    fp = open(responsekey_filename,"r")
    id = fp.readline()
    id.rstrip("\n")
    if id.isnumeric():
        lastresponseid = id 
    fp.close()
url = "www.claim.md"
connection = http.client.HTTPSConnection(url)
accountKey = "accountkey goes here"
content_type = "application/x-www-form-urlencoded"
h = {'Content-Type': content_type}

body = "AccountKey={accountKey}&ResponseID={lastresponseid}".format(accountKey=accountKey,lastresponseid=lastresponseid)

#print(h)
print(body)
connection.request("POST","/services/response/",body,h)

response = connection.getresponse()
xml = response.read().decode()
print (xml)

result = {}
root = ET.fromstring(xml)

if ("last_responseid" in root.attrib):
    result["last_responseid"] = root.attrib["last_responseid"]

result["claims"] = []
result["errors"] = []
for child in root:
    if child.tag == "claim":
        claim = {}
        for key in child.attrib:
            claim[key] = child.attrib[key]
        claim["messages"] = []
        for claimchild in child:
            if claimchild.tag == "messages":
                message = {}
                for messagekey in claimchild.attrib:
                    message[messagekey] = claimchild.attrib[messagekey]
                claim["messages"].append(message)
        result["claims"].append(claim)
    if child.tag == "error":
        error = {}
        for key in child.attrib:
            error[key] = child.attrib[key]
        result["errors"].append(error)
print (result)

# write the response id if received
if "last_responseid" in result:
    fp = open(responsekey_filename,"w")
    fp.write(result["last_responseid"])
    fp.close()


