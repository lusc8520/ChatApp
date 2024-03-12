using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.server.Plugins;

public class RandomJokePlugin : IPlugin
{
    private string _selectedJokeCategory = "Puns";
    public void Install(IChatClient chatClient)
    {
        chatClient.MessageSending += Execute;
    }

    private void Execute(IMessageDto messageDto)
    {
        if (messageDto.Text is "/joke")
        {
            var list = _jokesDictionry[_selectedJokeCategory];
            var joke = list[new Random().Next(list.Count)];
            messageDto.Text = joke;
        }
    }
    
    public void OpenPluginOptions()
    {
        var newWindow = new Window
        {
            Title = "Select Joke Category",
            Width = 250,
            Height = 300,
            Background = Brushes.DarkSlateGray
        };
        
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        var listBox = new ListBox
        {
            SelectionMode = SelectionMode.Single,
            Margin = new Thickness(10),
            Background = Brushes.DarkCyan,
            Foreground = Brushes.White
        };
        
        // Events for selecting a new joke category
        listBox.SelectionChanged += ListBox_SelectionChanged;
        listBox.MouseDoubleClick += ListBox_MouseDoubleClick;
        
        var jokeTypes = _jokesDictionry.Keys;
        foreach (var type in jokeTypes)
        {
            listBox.Items.Add(type);
        }
        scrollViewer.Content = listBox;
        newWindow.Content = scrollViewer;
        newWindow.Show();
    }
    
    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        HandleSelectionChange(sender as ListBox);
    }

    private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        HandleSelectionChange(sender as ListBox);
        Window.GetWindow(sender as ListBox)?.Close();
    }

    // set the new selected category
    private void HandleSelectionChange(ListBox? listBox)
    {
        var selectedCategory = listBox?.SelectedItem?.ToString();
        if (selectedCategory != null)
        {
            _selectedJokeCategory = selectedCategory;
            Console.WriteLine($"Selected Language: {_selectedJokeCategory}");
        }
    }
    
    private readonly Dictionary<string, List<string>> _jokesDictionry = new Dictionary<string, List<string>>(){
        {"Puns", new List<string>
            {
                "Why did the scarecrow win an award? Because he was outstanding in his field!",
                "I'm reading a book on anti-gravity. It's impossible to put down!",
                "I told my wife she was drawing her eyebrows too high. She looked surprised.",
                "Why don't skeletons fight each other? They don't have the guts.",
                "I'm on a seafood diet. I see food and I eat it.",
                "What do you call fake spaghetti? An impasta!",
                "I used to play piano by ear, but now I use my hands.",
                "The shovel was a ground-breaking invention.",
                "I'm trying to organize a hide and seek competition, but it's hard to find good players.",
                "The math professor went crazy with power because he had too many square roots."
            }
        },
        {"One-Liners", new List<string> 
                {
                "I told my computer I needed a break, now it won't stop sending me vacation ads.",
                "I'm reading a book on the history of glue. I just can't seem to put it down.",
                "I'm writing a book on reverse psychology. Please don't buy it.",
                "I asked the librarian if the library had any books on paranoia. She whispered, 'They're right behind you.'",
                "The easiest job in the world has to be a coroner. Surgery on dead people. What's the worst thing that could happen?",
                "I used to be a baker until I found out I was kneaded dough.",
                "I'm writing a book about hurricanes and tornadoes. It's a whirlwind of emotions.",
                "The future, the present, and the past walked into a bar. Things got tense.",
                "I used to play piano by ear, but now I use my hands.",
                "I told my wife she should embrace her mistakes. She gave me a hug." 
                }
        },
        {"Knock-Knock Jokes", new List<string>
            {
                "Knock knock. Who's there? Atch. Atch who? Bless you!",
                "Knock knock. Who's there? Boo. Boo who? Don't cry, it's just a joke!",
                "Knock knock. Who's there? Olive. Olive who? Olive you and I miss you!",
                "Knock knock. Who's there? Harry. Harry who? Harry up and let me in, it's cold out here!",
                "Knock knock. Who's there? Lettuce. Lettuce who? Lettuce in, it's freezing out here!",
                "Knock knock. Who's there? Doughnut. Doughnut who? Doughnut forget to let me in!",
                "Knock knock. Who's there? Cow says. Cow says who? No silly, a cow says moooo!",
                "Knock knock. Who's there? Cargo. Cargo who? No, car go beep beep!",
                "Knock knock. Who's there? Lettuce. Lettuce who? Lettuce in, it's cold out here!",
                "Knock knock. Who's there? Europe. Europe who? No, you're a poo!"
            }
        },
        {"Wordplay Jokes", new List<string>
            {
                "I told my wife she was drawing her eyebrows too high. She looked surprised.",
                "I'm trying to organize a hide and seek competition, but it's hard to find good players.",
                "The math professor went crazy with power because he had too many square roots.",
                "The shovel was a ground-breaking invention.",
                "I used to be a baker until I found out I was kneaded dough.",
                "I'm reading a book on anti-gravity. It's impossible to put down!",
                "Why don't skeletons fight each other? They don't have the guts.",
                "I'm on a seafood diet. I see food and I eat it.",
                "What do you call fake spaghetti? An impasta!",
                "I'm writing a book about hurricanes and tornadoes. It's a whirlwind of emotions."
            }
        },
        {"Silly Jokes", new List<string> 
            {
                "Why did the tomato turn red? Because it saw the salad dressing!",
                "What did one plate say to the other plate? Dinner is on me!",
                "What do you get when you cross a snowman and a vampire? Frostbite!",
                "Why did the golfer bring two pairs of pants? In case he got a hole in one!",
                "Why don't scientists trust atoms? Because they make up everything!",
                "What do you call fake spaghetti? An impasta!",
                "Why was the math book sad? Because it had too many problems.",
                "How does a penguin build its house? Igloos it together!",
                "Why did the bicycle fall over? Because it was two-tired!",
                "What's orange and sounds like a parrot? A carrot!"
            }
        },
        {"Animal Jokes", new List<string>
            {
                "Why did the chicken cross the playground? To get to the other slide!",
                "What do you call a fish with no eyes? Fsh!",
                "What do you call a bear with no teeth? A gummy bear!",
                "Why did the cow go to outer space? To see the moooon!",
                "Why did the squirrel swim on its back? To keep its nuts dry!",
                "What do you call a pig that knows karate? A pork chop!",
                "How do you catch a squirrel? Climb a tree and act like a nut!",
                "Why don't oysters donate to charity? Because they're shellfish!",
                "What do you call a dog magician? A labracadabrador!",
                "Why did the horse cross the road? To visit his neigh-bor!"
            }
            },
        {"Doctor/Health Jokes", new List<string>
            {
                "I told my doctor I broke my arm in two places. He told me to stop going to those places.", 
                "Why did the doctor carry a red pen? In case they needed to draw blood!", 
                "I told my dentist a good dentist's drill should never hurt. He said, 'That's why I always ask my patients to chew on my other hand!'", 
                "Why did the doctor carry a red pen? In case they needed to draw blood!", 
                "I told my dentist my teeth are going yellow. He told me to wear a brown tie.", 
                "Why was the doctor always calm? He had a lot of patients.", 
                "I told the doctor I broke my leg in two places. He told me to stop going to those places.", 
                "Why did the tomato turn red? Because it saw the salad dressing!", 
                "I told my doctor I broke my arm in two places. He told me to stop going to those places.", 
                "I told my dentist my teeth are going yellow. He told me to wear a brown tie."
            }
        },
        {"School Jokes", new List<string>
            {
                "Why did the math book look sad? Because it had too many problems.",
                "Why did the music teacher go to jail? Because she got caught with the lute.",
                "Why did the student eat his homework? Because the teacher said it was a piece of cake.",
                "Why did the scarecrow become a successful teacher? Because he was outstanding in his field!",
                "Why did the tomato turn red during class? Because it saw the salad dressing.",
                "Why did the pencil go to school? To get sharper!",
                "Why was the broom late for school? It overswept!",
                "Why did the clock get sent to the principal's office? For tocking too much!",
                "Why did the geometry book feel bad? Because it got too many compliments about its curves.",
                "Why was the math book sad? Because it had too many problems."
            }
        }
    };
}