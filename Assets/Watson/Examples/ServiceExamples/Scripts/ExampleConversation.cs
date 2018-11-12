﻿/**
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using UnityEngine;
using IBM.Watson.DeveloperCloud.Services.Conversation.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.Logging;
using System.Collections;
using FullSerializer;
using System.Collections.Generic;

public class ExampleConversation : MonoBehaviour
{
    private string _username = "8c04b265-2c6e-4e1b-bc92-25675b8dbadd";
    private string _password = "23vUxJldS5fZ";
    private string _url = "https://gateway.watsonplatform.net/conversation/api";
    private string _workspaceId = "862c4c18-def9-4107-8338-204a839e5224";

    private Conversation _conversation;
    private string _conversationVersionDate = "2017-05-26";

    private string[] _questionArray = { "can you turn up the AC", "can you turn on the wipers", "can you turn off the wipers", "can you turn down the ac", "can you unlock the door" };
    private fsSerializer _serializer = new fsSerializer();
    private Dictionary<string, object> _context = null;
    private int _questionCount = -1;
    private bool _waitingForResponse = true;

    void Start()
    {
        LogSystem.InstallDefaultReactors();

        //  Create credential and instantiate service
        Credentials credentials = new Credentials(_username, _password, _url);

        _conversation = new Conversation(credentials);
        _conversation.VersionDate = _conversationVersionDate;

        Runnable.Run(Examples());
    }

    private IEnumerator Examples()
    {
        if (!_conversation.Message(OnMessage, _workspaceId, "hello"))
            Log.Debug("ExampleConversation", "Failed to message!");

        while (_waitingForResponse)
            yield return null;

        _waitingForResponse = true;
        _questionCount++;

        AskQuestion();
        while (_waitingForResponse)
            yield return null;

        _questionCount++;

        _waitingForResponse = true;

        AskQuestion();
        while (_waitingForResponse)
            yield return null;
        _questionCount++;

        _waitingForResponse = true;

        AskQuestion();
        while (_waitingForResponse)
            yield return null;
        _questionCount++;

        _waitingForResponse = true;

        AskQuestion();
        while (_waitingForResponse)
            yield return null;

        Log.Debug("ExampleConversation", "Conversation examples complete.");
    }

    private void AskQuestion()
    {
        MessageRequest messageRequest = new MessageRequest()
        {
            input = new Dictionary<string, object>()
            {
                { "text", _questionArray[_questionCount] }
            },
            context = _context
        };

        if (!_conversation.Message(OnMessage, _workspaceId, messageRequest))
            Log.Debug("ExampleConversation", "Failed to message!");
    }

    private void OnMessage(object resp, string data)
    {
        Log.Debug("ExampleConversation", "Conversation: Message Response: {0}", data);

        //  Convert resp to fsdata
        fsData fsdata = null;
        fsResult r = _serializer.TrySerialize(resp.GetType(), resp, out fsdata);
        if (!r.Succeeded)
            throw new WatsonException(r.FormattedMessages);

        //  Convert fsdata to MessageResponse
        MessageResponse messageResponse = new MessageResponse();
        object obj = messageResponse;
        r = _serializer.TryDeserialize(fsdata, obj.GetType(), ref obj);
        if (!r.Succeeded)
            throw new WatsonException(r.FormattedMessages);

        //  Set context for next round of messaging
        object _tempContext = null;
        (resp as Dictionary<string, object>).TryGetValue("context", out _tempContext);

        if (_tempContext != null)
            _context = _tempContext as Dictionary<string, object>;
        else
            Log.Debug("ExampleConversation", "Failed to get context");
        _waitingForResponse = false;
    }
}