using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace ProxiCall.Bot.Dialogs.Shared
{
    public class StateAccessors
    {
        public IStatePropertyAccessor<LoggedUserState> LoggedUserAccessor { get; set; }
        public IStatePropertyAccessor<LuisState> LuisStateAccessor { get; set; }
        public IStatePropertyAccessor<CRMState> CRMStateAccessor { get; set; }
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        public ConversationState ConversationState { get; }
        public UserState UserState { get; }
        public PrivateConversationState PrivateConversationState { get; }

        public StateAccessors(UserState userState, ConversationState conversationState, PrivateConversationState privateConversationState)
        {
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState)); ;
            PrivateConversationState = privateConversationState ?? throw new ArgumentNullException(nameof(privateConversationState)); ;
        }
    }
}
