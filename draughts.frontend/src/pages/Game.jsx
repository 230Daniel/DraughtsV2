import React from "react";
import { Redirect, withRouter } from "react-router";

import Board from "../components/board/Board";
import MessageBox from "../components/MessageBox";

class Game extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			game: null,
			board: null,
			moveToAnimate: null,
			playerNumber: null,
			playerLeft: false
		};
		this.gameCode = this.props.match.params.gameCode;
	}

	render() {
		// If the state has been set to redirect the user, redirect them
		if (this.state.redirect) {
			return (
				<Redirect to={this.state.redirect} />
			);
		}

		// If the game hasn't been received from the server yet display a loading message
		if (!this.state.game) {
			return (
				<div className="container">
					<MessageBox title="Waiting for server..." load={true} />
				</div>
			);
		}

		var ableToTakeMove = this.state.game.type === 0 || this.state.game.board.nextPlayer === this.state.playerNumber;

		// Render the board and message box if necessary
		return (
			<div className="container">
				<Board
					board={this.state.board}
					moveToAnimate={this.state.moveToAnimate}
					flip={this.state.playerNumber === 0}
					readonly={!ableToTakeMove || this.state.moveToAnimate}
					onMoveTaken={(origin, destination) => this.onBoardMoveTaken(origin, destination)} />
				{this.renderMessageBox()}
			</div>
		);
	}

	renderMessageBox() {
		// If someone has won display a message box with the winner
		if (this.state.game.board.winner !== -1) {
			return (
				<MessageBox minWidth="350px" title={`${this.state.game.board.winner === 0 ? "Black" : "White"} won!`} link="/play" linkLabel="Back" />
			);
		} else if (this.state.playerLeft) {
			return (
				<MessageBox minWidth="350px" title="Error" message="Your opponent has disconnected" link="/play" linkLabel="Back" />
			);
		} else {
			return null;
		}
	}

	async componentDidMount() {
		// When the component mounts register a handler to the server's game updated event
		window._connection.on("GAME_UPDATED", this.handleOnGameUpdated);
		window._connection.on("PLAYER_LEFT", this.handleOnPlayerLeft);

		var response = await window._connection.invoke("READY", this.gameCode);
		if (response === -1) this.setState({ redirect: "/play" });

		this.setState({ playerNumber: response });
	}

	async componentWillUnmount() {
		// To avoid a memory leak, unregister the game updated event handler
		window._connection.off("GAME_UPDATED", this.handleOnGameUpdated);
		window._connection.off("PLAYER_LEFT", this.handlePlayerLeft);
		try {
			await window._connection.invoke("LEAVE_GAME", this.gameCode);
		} catch { }
	}

	handleOnGameUpdated = (game) => this.onGameUpdated(game);

	onGameUpdated(game) {

		if (!this.state.game) {
			this.setState({ game: game, board: game.board });
			return;
		}

		this.setState({ game: game });

		var moveToAnimate = game.board.turnMoves.at(-1);
		if (moveToAnimate) {
			this.setState({ moveToAnimate: moveToAnimate });
			setTimeout(() => {
				this.setState({ board: game.board, moveToAnimate: null });
			}, 210);
		} else {
			this.setState({ board: game.board });
		}
	}

	handleOnPlayerLeft = () => this.onPlayerLeft();

	onPlayerLeft() {
		this.setState({ playerLeft: true });
	}

	async onBoardMoveTaken(origin, destination) {
		// When the board tells us the player has taken a move, send it to the server
		await window._connection.invoke("TAKE_MOVE", this.gameCode, origin, destination);
	}
}

export default withRouter(Game);
