import React from "react";
import { Redirect, withRouter } from "react-router";

import Board from "../components/board/Board";
import MessageBox from "../components/MessageBox";

class Game extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			game: null,
			playerNumber: null
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

		if (this.state.game.type === 1 && this.state.game.status === 1) {
			return (
				<div className="container">
					<MessageBox minWidth="350px" title="Waiting for opponent..." message="Invite a friend using this game code" code={this.gameCode} link={"/play"} linkLabel={"Back"} />
				</div>
			);
		}

		// Render the board and message box if necessary
		return (
			<div className="container">
				<Board
					board={this.state.game.board}
					flip={this.state.playerNumber === 0}
					readonly={this.state.game.board.nextPlayer !== this.state.playerNumber}
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
		} else {
			return null;
		}
	}

	async componentDidMount() {
		// When the component mounts register a handler to the server's game updated event
		window._connection.on("GAME_UPDATED", this.handleOnGameUpdated);

		var response = await window._connection.invoke("JOIN_GAME", this.gameCode);

		if (!response.isSuccess)
			this.setState({ redirect: "/play" });

		this.setState({ playerNumber: response.playerNumber });
	}

	componentWillUnmount() {
		// To avoid a memory leak, unregister the game updated event handler
		window._connection.off("GAME_UPDATED", this.handleOnGameUpdated);
	}

	handleOnGameUpdated = (game) => this.onGameUpdated(game);

	onGameUpdated(game) {
		// When we receive a game updated event from the server, update the state with the new game
		this.setState({ game: game });
	}

	async onBoardMoveTaken(origin, destination) {
		// When the board tells us the player has taken a move, send it to the server
		await window._connection.invoke("TAKE_MOVE", this.gameCode, origin, destination);
	}
}

export default withRouter(Game);
