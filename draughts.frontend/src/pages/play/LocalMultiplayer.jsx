import React from "react";
import { Redirect } from "react-router";

import MessageBox from "../../components/MessageBox";

export default class LocalMultiplayer extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			redirect: null
		};
		this.gameCode = null;
	}

	render() {
		if (this.state.redirect) {
			return (
				<Redirect to={this.state.redirect} />
			);
		}

		return (
			<div className="container">
				<MessageBox title="Creating game..." load={true} />
			</div>
		);
	}

	async componentDidMount() {
		window._connection.on("GAME_STARTED", this.handleOnGameStarted);

		var code = await window._connection.invoke("CREATE_GAME", { gameType: 0 });
		this.gameCode = code;
		await window._connection.invoke("JOIN_GAME", code);
	}

	componentWillUnmount() {
		// To avoid a memory leak, unregister the game updated event handler
		window._connection.off("GAME_STARTED", this.handleOnGameStarted);
	}

	handleOnGameStarted = () => this.onGameStarted();

	onGameStarted() {
		this.setState({ redirect: `/game/${this.gameCode}` });
	}
}
