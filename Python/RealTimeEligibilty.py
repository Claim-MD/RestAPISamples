import http.client
import json
import os
import xml.etree.ElementTree as ET

def create_string_content(**kw):
    ret = ""
    ret += "--{boundary}\r\n".format(boundary=kw["boundary"])
    ret += "Content-Disposition: form-data; name=\"{name}\"\r\n".format(name=kw["name"])
    ret += "\r\n"
    ret += kw["value"] + "\r\n"
    return ret

def create_file_content(**kw):
    ret = ""
    ret += "--{boundary}\r\n".format(boundary=kw["boundary"])
    ret += "Content-Disposition: form-data; name=\"File\"; filename={filename}\r\n".format(filename=kw["filename"])
    ret += "\r\n"
    ret += kw["contents"] + "\r\n"
    return ret

def get_elig(**kw):
    boundary = "------------------------6e749b3a052e0543"
    accountKey = "account_key_goes_here"
    content_type = "multipart/form-data; boundary={boundary}".format(boundary=boundary)
    h = {'Content-Type': content_type}

    fp = open(kw["path"],"rb")
    filetext = fp.read().decode("ascii")
    fp.close()

    body = create_string_content(boundary=boundary,name="AccountKey",value=accountKey)
    body += create_file_content(boundary=boundary,filename=kw["filename"],contents=filetext)
    body += "--{boundary}--".format(boundary=boundary)
    #print(h)
    #print(body)

    url = "www.claim.md"
    connection = http.client.HTTPSConnection(url)
    connection.request("POST","/services/elig/",body,h)

    response = connection.getresponse()
    xml = response.read().decode()
    #print (xml)

    root = ET.fromstring(xml)
    for child in root:
        if child.tag == "eligibility":
            return (child.attrib["data"])

if __name__ == "__main__":
    data = get_elig(filename="sample.270",path="sample.270")
    print(data)