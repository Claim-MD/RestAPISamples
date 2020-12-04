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
    accountKey = "accountkey_goes_here"
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
    connection.request("POST","/services/upload/",body,h)

    response = connection.getresponse()
    xml = response.read().decode()
    #print (xml)

    result = {}
    root = ET.fromstring(xml)
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
    return result


if __name__ == "__main__":
    data = get_elig(filename="demo.837",path="demo.837")
    print(data)