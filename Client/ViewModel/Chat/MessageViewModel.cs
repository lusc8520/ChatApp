using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel.Chat;

public class MessageViewModel : ObservableObject, IMessageDto
{
    private string _text;
    
    public int Id { get; set; }
    public UserDto Sender { get; set; }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            OnPropertyChanged();
        }
    }
    public DateTime DateTime { get; set; }
    public int ChatId { get; set; }

    public static IMessageDto FromDto(IMessageDto messageDto)
    {
        if (messageDto is TextMessage)
        {
            return new MessageViewModel
            {
                Text = messageDto.Text,
                Id = messageDto.Id,
                ChatId = messageDto.ChatId,
                Sender = messageDto.Sender,
                DateTime = messageDto.DateTime
            };
        }
        return messageDto;
    }
}