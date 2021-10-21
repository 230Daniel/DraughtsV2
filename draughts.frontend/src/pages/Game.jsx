import React from "react";
import Board from "../components/board/Board";

export default class Game extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			game: null
		};
	}

	render(){
		if (!this.state.game) {
				return(
					<div className="container">
						<h1>Game!!!</h1>
					</div>
			);
		}

		return(
			<Board 
				board={this.state.game.board} 
				flip={false}
				onMoveTaken={(origin, destination) => this.onBoardMoveTaken(origin, destination)}/>
		)
	}

	async componentDidMount(){
		window._connection.on("GAME_UPDATED", this.handleOnGameUpdated);
		await window._connection.invoke("READY");
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
