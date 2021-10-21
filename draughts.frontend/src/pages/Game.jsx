import React from "react";
import { Redirect } from "react-router";

import Board from "../components/board/Board";
import MessageBox from "../components/MessageBox";

export default class Game extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			game: null
		};
	}

	render(){
		if (this.state.redirect) {
			return(
				<Redirect to={this.state.redirect}/>
			);
		}

		if (!this.state.game) {
			return(
				<div className="container">
					<MessageBox title="Waiting for server..." load={true}/>
				</div>
			);
		}

		return(
			<div className="container">
				<Board
					board={this.state.game.board} 
					flip={false}
					onMoveTaken={(origin, destination) => this.onBoardMoveTaken(origin, destination)}/>
				{this.renderMessageBox()}
			</div>
		)
	}

	renderMessageBox(){
		if (this.state.game.board.winner !== -1){
			return(
				<MessageBox title={`${this.state.game.board.winner === 0 ? "Black" : "White"} won!`} link="/play" linkLabel="Back"/>
			);
		} else {
			return null;
		}
	}

	async componentDidMount(){
		window._connection.on("GAME_UPDATED", this.handleOnGameUpdated);

		try {
			await window._connection.invoke("READY");
		} catch {
			this.setState({redirect: "/play"});
		}
	}

	componentWillUnmount(){
		window._connection.off("GAME_UPDATED", this.handleOnGameUpdated);
	}

	handleOnGameUpdated = (game) => this.onGameUpdated(game);

	onGameUpdated(game){
		this.setState({game: game});
	}

	async onBoardMoveTaken(origin, destination) {
		await window._connection.invoke("TAKE_MOVE", origin, destination);
	}
}
