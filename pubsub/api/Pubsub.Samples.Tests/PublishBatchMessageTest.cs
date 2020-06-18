﻿// Copyright 2020 Google Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Linq;
using Xunit;

[Collection(nameof(PubsubFixture))]
public class PublishBatchMessageTest
{
    private readonly PubsubFixture _pubsubFixture;
    private readonly PublishBatchedMessagesAsyncSample _publishBatchedMessagesAsyncSample;
    private readonly PullMessagesAsyncSample _pullMessagesAsyncSample;

    public PublishBatchMessageTest(PubsubFixture pubsubFixture)
    {
        _pubsubFixture = pubsubFixture;
        _pullMessagesAsyncSample = new PullMessagesAsyncSample();
        _publishBatchedMessagesAsyncSample = new PublishBatchedMessagesAsyncSample();
    }

    [Fact]
    public async void PublishBatchMessagesAsync()
    {
        string topicId = "testTopicForBatchMessageCreation" + _pubsubFixture.RandomName();
        string subscriptionId = "testSubscriptionForBatchMessageCreation" + _pubsubFixture.RandomName();

        _pubsubFixture.CreateTopic(topicId);
        _pubsubFixture.CreateSubscription(topicId, subscriptionId);

        List<string> messageTexts = new[] { "Hello World!", "Good day.", "Bye bye." }.ToList();

        var output = await _publishBatchedMessagesAsyncSample.PublishBatchMessagesAsync(_pubsubFixture.ProjectId, topicId, messageTexts);
        Assert.Equal(messageTexts.Count, output);

        // Pull the Message to confirm it is valid
        var result = await _pullMessagesAsyncSample.PullMessagesAsync(_pubsubFixture.ProjectId, subscriptionId, false);
        Assert.True(result > 0);
    }
}
