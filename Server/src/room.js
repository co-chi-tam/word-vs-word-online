
function GameRoom() {
    // Room data.
    this.roomName = '';
	this.roomTimer = 300;
	this.roomState = 'WAITING_PLAYER'; // WAITING_PLAYER | PLAYING_GAME | END_GAME
    // All players 
    this.players = [];
    this.maximumPlayers = 4;
    // LOGIC GAME
    this.turnLists = [];
    this.currentChar = 'a';

    // UPDATE PER SECONDS.
    this.updateTimer = function() {
		if (this.roomState != 'PLAYING_GAME')
			return;
		this.roomTimer -= 1;
		if (this.roomTimer <= 0)
		{
			this.roomState = 'END_GAME';
			this.endGame();
		} 
		else
		{
			this.emitAll ('countDownTimer', { roomTimer: this.roomTimer });
		}
    }

    // Get current turn.
    this.currentTurn = function() {
        return this.turnLists.length % this.maximumPlayers; 
    }

    // ADD TURN
    this.addTurn = function(playerName, word, suffix, point) {
        this.turnLists.push({ player: playerName, word: word, point: point });
        // UPDATE CURRENT CHAR
        this.currentChar = suffix;
    }
	
	// END GAME
	this.endGame = function() {
		var sum = {};
		for (let i = 0; i < this.players.length; i++) {
			const ply = this.players[i];
			if (typeof(sum [ply.player.playerName]) == 'undefined')
            {
                sum[ply.player.playerName] = { point: 0 };
            }
		}
		for (let i = 0; i < this.turnLists.length; i++) {
			const ply = this.turnLists[i];
			sum[ply.player].point += this.turnLists[i].point;
		}
		var results = [];
		for (let i = 0; i < this.players.length; i++) {
			const ply = this.players[i];
			results.push ({ playerName: ply.player.playerName, sum: sum[ply.player.playerName].point });
		}
		// RESULT
		this.emitAll ('endGameResult', { results: results });
		// CLEAR ROOM
		for (let i = 0; i < this.players.length; i++) {
            const ply = this.players[i];
			ply.room = null;
        }
        this.players = [];
        this.turnLists = [];
        this.roomState = 'WAITING_PLAYER';
    }

    // IS EXIST WORD
    this.isExistWord = function(word) {
        return this.turnLists.findIndex (i => i.word.trim() === word) > -1;
    }

    // Join room and set turn index for player
    this.join = function (player) {
        if (this.players.indexOf (player) == -1) {
            this.players.push (player);
            this.setTurnIndex();
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
        for (let i = 0; i < this.players.length; i++) {
            const ply = this.players[i];
            ply.emit('clearRoom', {
                msg: "Room is empty or player is quit."
            });
        }
        this.players = [];
        this.turnLists = [];
        this.currentChar = 'a';
        this.roomState = 'WAITING_PLAYER';
		this.roomTimer = 600;
    };
    
    // Leave room 
    this.leave = function(player) {
        if (this.players.indexOf (player) > -1) {
            this.players.splice (this.players.indexOf (player), 1);
            console.log ('User LEAVE ROOM...' + player.player);
        }
    };
    
    // Send all mesg for players in room.
    this.emitAll = function (name, obj) {
        for (let i = 0; i < this.players.length; i++) {
            const player = this.players[i];
            player.emit(name, obj);
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