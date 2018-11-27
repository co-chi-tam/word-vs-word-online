const GameRoom = require('./room'); // ROOM LOGIC
const profaneJs = require("profane-js")

const MAXIMUM_ROOMS = 10; // Maximum rooms
const MAXIMUM_PLAYERS = 4; // Maximum players
const MAXIMUM_TIME_PLAY = 180; // Maximum time play
const TIME_TO_ANSWER = 30;

users = []; // Array User names
rooms = {}; // Rooms
roomStatus = new Array(MAXIMUM_ROOMS); // ROOM STATUS

// Limit word can play.
character_list = ['a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'];
words_list = {}; // WORDS

var GameXO = function (http) {
	activeTimer();
    var io = require('socket.io')(http); // Require socket.io
    var fs = require('fs');
    var wordCorrectPath = './public/word_correct.txt';
    // console.log (words_listf-ck[0].trim() === 'a');
    // console.log (words_list.findIndex (i => i.trim() === 'afd'));
    // READ DATA WORD
    fs.readFile(wordCorrectPath, function(err, data) {
        if(err) throw err;
        var array = data.toString().split("\n");
        for (i in array) {
			if (array[i].length > 2)
			{
				var prefix = array[i].substring(0, 1);
				if (typeof(words_list [prefix]) == 'undefined')
				{
					words_list [prefix] = [];
				}
				words_list [prefix].push(array[i].toString());
			}
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
        if (word.length < 2)
            return false;
        if (word.length > 22)
            return false;
        var prefix = word.substring(0, 1);
        var listWords = getWordsWith(prefix);
        return listWords.findIndex (i => i.trim() === word) > -1;
    }
	
	function refreshWord(word)
	{
		if (typeof(word) == 'undefined')
			return '';
		return word.replace(/[`~!@#$%^&*()_|+\-=?;:'",.<>\{\}\[\]\\\/]/gi, '');
	}
	
	function isBadWord(word)
	{
		return profaneJs.containsProfanity(word);
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
            + '<color=#FF004F>' + word.substring(word.length - 1, word.length).toLowerCase() + '</color>';
    }
	
	function updateRoomStatus()
	{
		for (let i = 0; i < MAXIMUM_ROOMS; i++) {
			const roomName = 'Room-' + (i + 1);
			const playerCount = typeof (rooms [roomName]) !== 'undefined' 
									? rooms [roomName].length()
									: 0;
			roomStatus[i] = {
				roomName: roomName,
				players: playerCount,
				maxPlayers: MAXIMUM_PLAYERS
			};
		}
	}

    // On client connect.
    io.on('connection', function(socket) {
        // console.log('A user connected ' + (socket.client.id));
        // Welcome message
        socket.emit('welcome', { 
            msg: 'Welcome to connect game word fight online.'
        });
        // INIT PLAYER
        // Set player name.
        socket.on('setPlayerName', function(data) {
            if (data) 
			{
                var isDuplicateName = false;
				var playerName = refreshWord(data.playerName);
				if (isBadWord(playerName) == false)
				{
					for (let i = 0; i < users.length; i++) {
						const u = users[i];
						if (u.playerName == playerName) {
							isDuplicateName = true;    
							break;
						}
					}
					if(isDuplicateName) {
						socket.emit('msgError', { 
							msg: playerName  + ' username is taken! Try another username.'
						});
					} else {
						if (playerName.length < 5 || playerName.length > 18) {
							socket.emit('msgError', { 
								msg: playerName  + ' username must longer than 5 character'
							});
						} else {
							socket.player = {
								playerAvatar: parseInt (data.playerAvatar),
								playerName: playerName,
								playerFrame: parseInt (data.playerFrame)
							};
							users.push(socket.player);
							socket.emit('playerNameSet', { 
								id: socket.client.id,
								name: playerName 
							});
						}
					}
				}
				else
				{
					socket.emit('msgError', { 
						msg: playerName  + ' username is not available! Try another username.'
					});
				}
            }
        });
		// Set player data.
        socket.on('updatePlayerData', function(data) {
			if (data && socket.player) 
			{
				socket.player.playerAvatar = parseInt (data.playerAvatar);
				socket.player.playerName = data.playerName;
				socket.player.playerFrame = parseInt (data.playerFrame);
			}
		});
        // Receive beep mesg
        socket.on('beep', function(data) {
			socket.emit('boop');
        })
        // INIT ROOM
        // Get all room status
        socket.on('getRoomsStatus', function() {
			// UPDATE ROOM STATUS
            updateRoomStatus();
			// SEND TO CLIENT
            socket.emit('updateRoomStatus', {
                rooms: roomStatus
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
							roomName: roomName,
                            msg: "Join room completed."
                        });
						// ALL MEMBERS IN ROOM
                        rooms [roomName].emitAll('newJoinRoom', {
                            roomInfo: rooms [roomName].getInfo()
                        });
                        socket.room = rooms [roomName]; 
                        // console.log ("A player join room. " + roomName + " Room: " + rooms [roomName].length());
						// UPDATE
						updateRoomStatus();
						// SEND TO ALL CLIENT
						io.sockets.emit('updateRoomSize', {
							roomName: rooms [roomName].roomName,
							players: rooms [roomName].length(),
							maxPlayers: MAXIMUM_PLAYERS
						});
                        // START GAME
                        if (rooms [roomName].length() == MAXIMUM_PLAYERS) {
                            // UPDATE INDEX
                            rooms [roomName].setTurnIndex();
                            // FIRST DATA
                            var firstChar = getCharacterRandom();
                            // DATA
                            rooms [roomName].currentChar = firstChar;
                            rooms [roomName].answerTimer = TIME_TO_ANSWER;
                            rooms [roomName].timeLimitToAnswer = TIME_TO_ANSWER;
							// STATE
							rooms [roomName].roomState = 'PLAYING_GAME';
                            rooms [roomName].roomTimer = MAXIMUM_TIME_PLAY;
                            // EMIT START GAME
                            rooms [roomName].emitAll('startGame', {
								goldCost: 3,
                                firstCharacter: firstChar,
                                firstPlayerIndex: 0,
                                roomState: rooms [roomName].roomState,
								roomInfo: rooms [roomName].getInfo()
                            });
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
            if(msg && socket.room && socket.player) {
				var goodMsg = refreshWord(msg.message);
				if (isBadWord(goodMsg) == false)
				{
					socket.room.emitAll('msgChatRoom', {
						user: socket.player.playerName,
						message: goodMsg
					});
				}	
            }
        });
        // Receive world chat.
        socket.on('sendWorldChat', function(msg) {
            if(msg && socket.player) {
				var goodMsg = refreshWord(msg.message);
                // socket.broadcast.emit => will send the message to all the other clients except the newly created connection
				if (isBadWord(goodMsg) == false)
				{
					io.sockets.emit('msgWorldChat', {
						user: socket.player.playerName,
						message: goodMsg
					});
				}
            }
        });
        // Receive leave room mesg.
        socket.on('leaveRoom', function() {
            // console.log ('User leave room...' + socket.id);
            // LEAVE ROOM
            leaveRoom(socket);
        });
		// Suggest word
        socket.on('suggestWord', function(data) {
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
            // console.log ('User send word. ' + JSON.stringify(data));
            if(data && socket.room) {
                // IS PLAYING ROOM
                if (socket.room.roomState == 'PLAYING_GAME')
                {
                    var turnIndex = data.turnIndex;
					var goodWord = refreshWord(data.word);
                    var word = goodWord.toLowerCase();
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
                            socket.room.addTurn(socket.player, calWord.suffix, word, 1);
							// ADD GOLD
							socket.emit('addGold', { gold: 1 });
                            // EMIT ALL WORD TO CLIENTS
                            socket.room.emitAll('allRoomGetWord', {
								goldCost: 3,
								playerAvatar: socket.player.playerAvatar,
								playerName: socket.player.playerName,
								playerFrame: socket.player.playerFrame,
								answerTimer: TIME_TO_ANSWER,
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
                                msg: word + ' is not available.'
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
            // console.log ('User disconnect...' + socket.id);
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
                socket.room.leave(socket); // ROOM REMOVE SOCKET
                socket.emit ('leaveRoom');
				socket.room.endGame();
			}
			else
			{
                socket.room.leave(socket); // ROOM REMOVE SOCKET
                socket.emit ('leaveRoom');
				if (socket.room.length() > 0)
				{
                    socket.room.emitAll('newLeaveRoom', { roomInfo: socket.room.getInfo() }); // OTHER GET NEW INFO
                    // RENEW INDEX
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
			// UPDATE ROOM STATUS
            updateRoomStatus();
        }, 1000);
    }
};
// INIT
module.exports = GameXO;