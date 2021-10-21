import React from "react";

export default class Game extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			
		};
	}

	render(){
		return(
			<div className="container">
				<h1>Game!!!</h1>
			</div>
		);
	}

	onGameStarted(game){
		console.log(game);
	}

	async componentDidMount(){
		window._connection.on("GAME_STARTED", this.onGameStarted);
		await window._connection.invoke("READY");
	}

	componentWillUnmount(){
		window._connection.off("GAME_STARTED", this.onGameStarted);
	}
}
