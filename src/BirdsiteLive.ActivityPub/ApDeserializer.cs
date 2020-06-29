﻿using Newtonsoft.Json;

namespace BirdsiteLive.ActivityPub
{
    public class ApDeserializer
    {
        public static Activity ProcessActivity(string json)
        {
            var activity = JsonConvert.DeserializeObject<Activity>(json);
            switch (activity.type)
            {
                case "Follow":
                    return JsonConvert.DeserializeObject<ActivityFollow>(json);
                case "Undo":
                    var a = JsonConvert.DeserializeObject<ActivityUndo>(json);
                    if(a.apObject.type == "Follow")
                        return JsonConvert.DeserializeObject<ActivityUndoFollow>(json);
                    break;
                case "Accept":
                    var accept = JsonConvert.DeserializeObject<ActivityAccept>(json);
                    //var acceptType = JsonConvert.DeserializeObject<Activity>(accept.apObject);
                    switch ((accept.apObject as dynamic).type.ToString())
                    {
                        case "Follow":
                            var acceptFollow = new ActivityAcceptFollow()
                            {
                                type = accept.type,
                                id = accept.id,
                                actor = accept.actor,
                                context = accept.context,
                                apObject = new ActivityFollow()
                                {
                                    id = (accept.apObject as dynamic).id?.ToString(),
                                    type = (accept.apObject as dynamic).type?.ToString(),
                                    actor = (accept.apObject as dynamic).actor?.ToString(),
                                    context = (accept.apObject as dynamic).context?.ToString(),
                                    apObject = (accept.apObject as dynamic).@object?.ToString()
                                }
                            };
                            return acceptFollow;
                            break;
                    }
                    break;
            }

            return null;
        }

        private class Ac : Activity
        {
            [JsonProperty("object")]
            public Activity apObject { get; set; }
        }
    }
}