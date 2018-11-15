
const DELTA_TIMER = 1;

function GameRoom() {
    // Room data.
    this.roomName = '';
    this.roomTimer = 300;

    this.timeLimitToAnswer = 30;
    this.answerTimer = 30;

	this.roomState = 'WAITING_PLAYER'; // WAITING_PLAYER | PLAYING_GAME | END_GAME
    // All players 
    this.players = [];
    this.maximumPlayers = 4;
    // LOGIC GAME
    this.turnLists = [];
    this.currentChar = 'a';

    // UPDATE PER SECONDS.
    this.updateTimer = function() {
		if (this.roomState == 'PLAYING_GAME')
		{
			this.roomTimer -= DELTA_TIMER;
			if (this.roomTimer <= 0)
			{
				this.roomState = 'END_GAME';
				this.endGame();
			} 
			else
			{
				// TIMER TO ROOM
				this.emitAll ('countDownTimer', { roomTimer: this.roomTimer });
				// TIMER TO ANSWER
				var currentPlayer = this.currentPlayer();
				if (currentPlayer)
				{
					if (this.answerTimer > 0)
					{
						currentPlayer.emit("counterDownAnswer", { answerTimer: this.answerTimer });
					}
					else
					{
						this.passTurn(currentPlayer.player);
					}
					this.answerTimer -= DELTA_TIMER;
				}
			}
		}
    }

    // Get current turn.
    this.currentTurn = function() {
        return this.turnLists.length % this.maximumPlayers; 
    }

    // GET CURRENT PLAYER
    this.currentPlayer = function() {
        var index = this.currentTurn();
        if (index > -1)
        {
            return this.players[index];
        }
        return null;
    }

    // ADD TURN
    this.addTurn = function(player, suffix, word, point) {
        this.turnLists.push({ player: player, word: word, point: point });
        // TIMER
        this.answerTimer = this.timeLimitToAnswer;
        // UPDATE CURRENT CHAR
        this.currentChar = suffix;
    }

    // PASS TURN
    this.passTurn = function(player)
    {
        // PASS VALUE
        this.turnLists.push({ player: player, word: '#####', point: 0 });
        // TIMER
        this.answerTimer = this.timeLimitToAnswer;
        // EMIT PASS TURN
        var currentTurn = this.currentTurn(); 
        this.emitAll('onePassTurn', {
            goldCost: 3,
            lastIndex: currentTurn - 1,
            turnIndex: currentTurn,
            nextCharacter: this.currentChar
        });
    }
	
	// END GAME
	this.endGame = function() {
		var sum = {};
		for (let i = 0; i < this.players.length; i++) {
			const ply = this.players[i];
			if (ply && typeof(sum [ply.player.playerName]) == 'undefined')
            {
                sum[ply.player.playerName] = { point: 0 };
            }
		}
		for (let i = 0; i < this.turnLists.length; i++) {
			const turn = this.turnLists[i];
			if (turn.player && typeof(sum [turn.player.playerName]) != 'undefined')
            {
				sum[turn.player.playerName].point += turn.point;
			}
		}
		var results = [];
		for (let i = 0; i < this.players.length; i++) {
			const ply = this.players[i];
			if (ply)
			{
				results.push ({ player: ply.player, sum: sum[ply.player.playerName].point });
			}
		}
		// RESULT
		this.emitAll ('endGameResult', { roomState: this.roomState, results: results });
		// CLEAR ROOM
		for (let i = 0; i < this.players.length; i++) {
            const ply = this.players[i];
			if (ply)
			{
				ply.room = null;
			}
        }
        this.players = [];
        this.turnLists = [];
        this.roomState = 'WAITING_PLAYER';
    }

    // IS EXIST WORD
    this.isExistWord = function(word) {
        return this.turnLists.findIndex (i => i.word.trim() == word.trim()) > -1;
    }

    // Join room and set turn index for player
    this.join = function (player) {
        if (this.players.indexOf (player) == -1) {
            this.players.push (player);
        }
    };
	
	// TURN INDEX
	this.setTurnIndex = function() {
		for (let i = 0; i < this.players.length; i++) {
			const ply = this.players[i];
			ply.game = {
				turnIndex: i
			};
			ply.emit('turnIndexSet', {
				turnIndex: i
			});
		}
	}

    // Clear room
    this.clearRoom = function() {
        this.emitAll ('clearRoom', {
            msg: "Room is empty or player disconnected."
        });
        this.players = [];
        this.turnLists = [];
        this.currentChar = 'a';
        this.roomState = 'WAITING_PLAYER';
		this.roomTimer = 600;
    };
    
    // Leave room 
    this.leave = function(player) {
        var index = this.players.indexOf (player);
        if (index > -1) {
            this.players.splice (index, 1);
            // console.log ('User LEAVE ROOM...' + player.player);
        }
    };
    
    // Send all mesg for players in room.
    this.emitAll = function (name, obj) {
        for (let i = 0; i < this.players.length; i++) {
            const player = this.players[i];
            player.emit(name, obj);
        }
    };

    // Send all mesg for players in room except one.
    this.emitAllExcept = function (socket, name_sok, obj_sok, name_oth, obj_oth) {
        var index = this.players.indexOf (socket);
        // console.log ('emitAllExcept ' + index);
        for (let i = 0; i < this.players.length; i++) {
            const player = this.players[i];
            // IS SOCKET
            if (i == index)
            {
                if (obj_sok)
                    player.emit(name_sok, obj_sok);
                else
                    player.emit(name_sok);
            }
            // OTHER SOCKET
            else
            {
                if (obj_oth)
                    player.emit(name_oth, obj_oth);
                else
                    player.emit(name_oth);
            }
        }
    };

    // Get rom info.
    this.getInfo = function() {
        var playerInfoes = [];
        for (let i = 0; i < this.players.length; i++) {
            const player = this.players[i];
            playerInfoes.push (player.player);
        }
        return {
            roomName: this.roomName,
            players: playerInfoes
        };
    }
	
	// Each client contain in room.
    this.each = function (callback) {
		for (let i = 0; i < this.players.length; i++) {
            const player = this.players[i];
			if (callback)
			{
				callback(player);
			}
        }
    };

    // If client contain in room.
    this.contain = function (player) {
        return this.players.indexOf (player) > -1;
    };
    
    // Get amount of players in room.
    this.length  = function () {
        return this.players.length;
    };
};
// INIT
module.exports = GameRoom;