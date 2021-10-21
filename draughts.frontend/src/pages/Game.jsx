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
			<Board/>
		)
	}

	async componentDidMount(){
		window._connection.on("GAME_STARTED", this.handleOnGameStarted);
		await window._connection.invoke("READY");
	}

	componentWillUnmount(){
		window._connection.off("GAME_STARTED", this.handleOnGameStarted);
	}

	handleOnGameStarted = (game) => this.onGameStarted(game);

	onGameStarted(game){
		console.log(this);
		this.setState({game: game});
	}
}
