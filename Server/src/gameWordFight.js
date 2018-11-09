
const GameRoom = require('./room'); // ROOM LOGIC
const MAXIMUM_ROOMS = 10; // Maximum rooms
const MAXIMUM_PLAYERS = 4; // Maximum players
const MAXIMUM_TIME_PLAY = 300; // Maximum time play

users = []; // Array User names
rooms = {}; // Rooms
// Limit word can play.
character_list = ['a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'];
words_list = {}; // WORDS

var GameXO = function (http) {
	activeTimer();
    var io = require('socket.io')(http); // Require socket.io
    var fs = require('fs');
    var wordCorrectPath = './public/word_correct.txt';
    // console.log (words_list[0].trim() === 'a');
    // console.log (words_list.findIndex (i => i.trim() === 'afd'));
    // READ DATA WORD
    fs.readFile(wordCorrectPath, function(err, data) {
        if(err) throw err;
        var array = data.toString().split("\n");
        for (i in array) {
            var prefix = array[i].substring(0, 1);
            if (typeof(words_list [prefix]) == 'undefined')
            {
                words_list [prefix] = [];
            }
            words_list [prefix].push(array[i].toString());
        }
    });

    function getWordsWith(prefix)
    {
        if (typeof(words_list [prefix]) == 'undefined')
            return null;
        return words_list [prefix];
    }

    function getWordsWithRange(prefix, min, max)
    {
        if (typeof(words_list [prefix]) == 'undefined')
            return null;
        return getWordsWith(prefix).splice(min, max);
    }

    function getCharacterRandom()
    {
        var random = getRandomWith(0, character_list.length - 1);
        return character_list [random];
    }

    function getWordRandom()
    {
        var prefix = getCharacterRandom();
        return getWordWithRandom(prefix);
    }

    function getWordWithRandom(prefix)
    {
        if (typeof(words_list [prefix]) == 'undefined')
            return null;
        var random = getRandomWith(0, words_list [prefix].length);
        return getWordsWith(prefix)[random];
    }

    function getRandomWith(min, max) {
        return Math.floor(Math.random() * (max - min + 1)) + min;
    }

    function isWordInList(word)
    {
        var prefix = word.substring(0, 1);
        var listWords = getWordsWith(prefix);
        return listWords.findIndex (i => i.trim() === word) > -1;
    }

    function analyticWord(word)
    {
        return {
            prefix: word.substring(0, 1),
            middle: word.substring(1, word.length - 1),
            suffix: word.substring(word.length - 1, word.length)
        };
    }

    function displayWord(word)
    {
        return '<color=#0074FF>' + word.substring(0, 1).toUpperCase() + '</color>' 
			+ word.substring(1, word.length - 1).toLowerCase() 
            + '<color=#FF004F>' + word.substring(word.length - 1, word.length).toUpperCase() + '</color>';
    }

    // On client connect.
    io.on('connection', function(socket) {
        console.log('A user connected ' + (socket.client.id));
        // Welcome message
        socket.emit('welcome', { 
            msg: 'Welcome to connect game word fight online.'
        });
        // INIT PLAYER
        // Set player name.
        socket.on('setPlayerName', function(data) {
            if (data && data.playerName) {
                var isDuplicateName = false;
                for (let i = 0; i < users.length; i++) {
                    const u = users[i];
                    if (u.playerName == data.playerName) {
                        isDuplicateName = true;    
                        break;
                    }
                }
                if(isDuplicateName) {
                    socket.emit('msgError', { 
                        msg: data.playerName  + ' username is taken! Try another username.'
                    });
                } else {
                    if (data.playerName.length < 5 || data.playerName.length > 18) {
                        socket.emit('msgError', { 
                            msg: data.playerName  + ' username must longer than 5 character'
                        });
                    } else {
                        socket.player = {
                            playerAvatar: data.playerAvatar,
                            playerName: data.playerName
                        };
                        users.push(data);
                        socket.emit('playerNameSet', { 
                            id: socket.client.id,
                            name: data.playerName 
                        });
                    }
                }
            }
        });
        // Receive beep mesg
        socket.on('beep', function(data) {
        socket.emit('boop');
        })
        // INIT ROOM
        // Get all room status
        socket.on('getRoomsStatus', function() {
            var results = [];
            for (let i = 0; i < MAXIMUM_ROOMS; i++) {
                const roomName = 'Room-' + (i + 1);
                const playerCount = typeof (rooms [roomName]) !== 'undefined' 
                                        ? rooms [roomName].length()
                                        : 0;
                results.push ({
                    roomName: roomName,
                    roomDisplay: roomName + ': ' + playerCount + '/' + MAXIMUM_PLAYERS,
                    players: playerCount,
                    maxPlayers: MAXIMUM_PLAYERS
                });
            }
            socket.emit('updateRoomStatus', {
                rooms: results
            });
        });
        // Join or create room by name. 
        socket.on('joinOrCreateRoom', function(playerJoin) {
            if(playerJoin && socket.player) {
                var roomName = playerJoin.roomName;
                if (typeof(rooms [roomName]) === 'undefined') {
                    rooms [roomName] = new GameRoom();
					rooms [roomName].roomState = 'WAITING_PLAYER';
                }
                rooms [roomName].roomName = roomName;
                rooms [roomName].maximumPlayers = MAXIMUM_PLAYERS;
                if (rooms [roomName].contain (socket) == false) {
                    // console.log (" Room: " + rooms [roomName].length() + " / " + rooms [roomName].roomState);
                    if (rooms [roomName].length() < MAXIMUM_PLAYERS 
						&& rooms [roomName].roomState != 'PLAYING_GAME') {
                        // JOIN
                        rooms [roomName].join (socket);
						socket.emit('joinRoomCompleted', {
                            msg: "Join room completed."
                        });
						// ALL MEMBERS IN ROOM
                        rooms [roomName].emitAll('newJoinRoom', {
                            roomInfo: rooms [roomName].getInfo()
                        });
                        socket.room = rooms [roomName]; 
                        console.log ("A player join room. " + roomName + " Room: " + rooms [roomName].length());
                        // START GAME
                        if (rooms [roomName].length() == MAXIMUM_PLAYERS) {
                            var firstChar = getCharacterRandom();
                            rooms [roomName].currentChar = firstChar;
                            rooms [roomName].emitAll('startGame', {
								goldCost: 3,
                                firstCharacter: firstChar,
                                firstPlayerIndex: 0,
								roomInfo: rooms [roomName].getInfo()
                            });
							// STATE
							rooms [roomName].roomState = 'PLAYING_GAME';
							rooms [roomName].roomTimer = MAXIMUM_TIME_PLAY;
                        }
						else
						{
							// STATE
							rooms [roomName].roomState = 'WAITING_PLAYER';
						}
                    } else {
                        socket.emit('joinRoomFailed', {
                            msg: "Room is full. Please try again late."
                        });
                    }
                } else {
                    socket.emit('joinRoomFailed', {
                        msg: "You are already join room."
                    });
                }
            }
        });
        // Receive client chat in current room.
        socket.on('sendRoomChat', function(msg) {
            if(msg && socket.room) {
                socket.room.emitAll('msgChatRoom', {
                    user: socket.player.playerName,
                    message: msg.message
                });
            }
        });
        // Receive world chat.
        socket.on('sendWorldChat', function(msg) {
            if(msg) {
                // socket.broadcast.emit => will send the message to all the other clients except the newly created connection
                io.sockets.emit('msgWorldChat', {
                    user: socket.player.playerName,
                    message: msg.message
                });
            }
        });
        // Receive leave room mesg.
        socket.on('leaveRoom', function() {
            console.log ('User leave room...' + socket.id);
            // LEAVE ROOM
            leaveRoom(socket);
        });
		// Suggest word
        socket.on('suggestWord', function(data) {
            console.log ('suggestWord...' + socket.id);
			if (socket.room) 
			{
				if (socket.room.roomState == 'PLAYING_GAME')
                {
					var prefix = data.prefix.toLowerCase().trim();
					var listWord = getWordsWith(prefix);
					var result = '';
					for (var i in listWord)
					{
						if (socket.room.isExistWord(listWord[i]) == false)
						{
							result = listWord[i];
							break;
						}
					}
					// SUGGEST WORD
					socket.emit('receiveSuggestWord', { wordSuggest: result, goldCost: 3 });
				}
			}
        });
        // GAME PLAY
        socket.on('sendWord', function(data) {
            console.log ('User send word. ' + JSON.stringify(data));
            if(data && socket.room) {
                // IS PLAYING ROOM
                if (socket.room.roomState == 'PLAYING_GAME')
                {
                    var turnIndex = data.turnIndex;
                    var word = data.word.toLowerCase();
                    var calWord = analyticWord(word);
                    // IS CORRECT TURN
                    if (socket.room.currentTurn() == turnIndex)
                    {
                        // IS WORD CORRECT
                        if (isWordInList(word)
                            && calWord.prefix.trim() == socket.room.currentChar.trim()  // IS CHAR CORRECT
                            && socket.room.isExistWord(word) == false) // WORD PLAYED
                        {
                            // ADD TURN
                            socket.room.addTurn(socket.player.playerName, word, calWord.suffix, 1);
							// ADD GOLD
							socket.emit('addGold', { gold: 1 });
                            // EMIT ALL WORD TO CLIENTS
                            socket.room.emitAll('allRoomGetWord', {
								goldCost: 3,
								playerAvatar: socket.player.playerAvatar,
								playerName: socket.player.playerName,
								lastIndex: turnIndex,
                                turnIndex: socket.room.currentTurn(),
                                nextCharacter: calWord.suffix,
                                displayWord: displayWord(word)
                            });
                        }
                        else
                        {
                            // ERROR
                            socket.emit('msgError', { 
                                msg: word + ' is not correct.'
                            });
                        }
                    }
                    else
                    {
                        // ERROR
                        socket.emit('msgError', { 
                            msg: 'This is not your turn. Please wait.'
                        });
                    }
                }
                else
                {
                    // ERROR
                    socket.emit('msgError', { 
                        msg: 'Game is not start or ended.'
                    });
                }
            }
        });
        // DISCONNECT
        // Disconnect and clear room.
        socket.on('disconnect', function() {
            console.log ('User disconnect...' + socket.id);
            if (socket.player) {
                for (let i = 0; i < users.length; i++) {
                    const u = users[i];
                    if (u.playerName == socket.player.playerName) {
                        users.splice(i, 1);  
                        break;
                    }
                }
            }
            // LEAVE ROOM
            leaveRoom(socket);
        });
    });
    
    // LEAVE ROOM
	function leaveRoom(socket)
	{
		if (socket.room) 
		{
			if (socket.room.roomState != 'WAITING_PLAYER')
			{
				socket.room.clearRoom();
				socket.room.roomState = 'END_GAME';
				socket.room = null;
				// var roomName = socket.room.roomName;
				// delete rooms [roomName];
			}
			else
			{
				socket.room.leave(socket);
				socket.emit('leaveRoom');
				if (socket.room.length() > 0)
				{
					socket.room.emitAll('newLeaveRoom', {
						roomInfo: socket.room.getInfo()
					});
					socket.room.setTurnIndex();
				}
				else
				{
					socket.room.clearRoom();
					socket.room.roomState = 'END_GAME';
					socket.room = null;
					// var roomName = socket.room.roomName;
					// delete rooms [roomName];
				}
			}
		}
		else
		{
			// ERROR
			socket.emit('msgError', { 
				msg: 'You not join room.'
			});
		}
    }
    // INTERVAL TIMER 
    function activeTimer()
    {
        this.roomTimer = setInterval(function() {
            for (let i = 0; i < MAXIMUM_ROOMS; i++) {
                const roomName = 'Room-' + (i + 1);
                if (typeof (rooms [roomName]) !== 'undefined')
                {
                    rooms [roomName].updateTimer();
                }
            }
        }, 1000);
    }
};
// INIT
module.exports = GameXO;