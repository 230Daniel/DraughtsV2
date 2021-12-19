import React from "react";
import Loader from "react-loader-spinner";
import { Redirect } from "react-router";

import styles from "./Play.Module.css";

export default class Computer extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			redirect: null,

			side: -1,
			engine: 0,
			engineThinkingTime: 2000
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
			<div className={styles.container}>
				<div className={styles.row}>
					<div className={styles.box}>
						<span className={styles.title}>Choose game options</span>
						<form onSubmit={(e) => this.onSubmitCreateGame(e)}>
							<div className={styles.inputGroup}>
								<label>Play as side</label>
								<select
									value={this.state.side}
									onChange={(e) => this.setState({ side: parseInt(e.target.value) })}>
									<option value={-1}>Random</option>
									<option value={0}>Black</option>
									<option value={1}>White</option>
								</select>
							</div>
							<div className={styles.inputGroup}>
								<label>Engine</label>
								<select
									value={this.state.engine}
									onChange={(e) => this.setState({ engine: parseInt(e.target.value) })}>
									<option value={0}>MiniMax</option>
									<option value={1}>Random Moves</option>
								</select>
							</div>
							{this.state.engine == 0 &&
								<>
									<div className={styles.inputGroup}>
										<label>Thinking time</label>
										<select
											value={this.state.engineThinkingTime}
											onChange={(e) => this.setState({ engineThinkingTime: parseInt(e.target.value) })}>
											<option value={1000}>1 second</option>
											<option value={2000}>2 seconds</option>
											<option value={3000}>3 seconds</option>
											<option value={4000}>4 seconds</option>
											<option value={5000}>5 seconds</option>
										</select>
									</div>
								</>
							}
							{!this.state.creatingGame &&
								<button className={styles.button} type="submit">Create</button>
							}
							{this.state.creatingGame &&
								<Loader className={styles.loader} color="#ffffff" type="ThreeDots" height={30} />
							}
						</form>
					</div>
				</div>
			</div>
		);
	}

	componentDidMount() {
		window._connection.on("GAME_STARTED", this.handleOnGameStarted);
	}

	componentWillUnmount() {
		// To avoid a memory leak, unregister the game updated event handler
		window._connection.off("GAME_STARTED", this.handleOnGameStarted);
	}

	async onSubmitCreateGame(e) {
		e.preventDefault();

		this.setState({ creatingGame: true });
		var code = await window._connection.invoke("CREATE_GAME", { gameType: 2, side: this.state.side, engine: this.state.engine, engineThinkingTime: this.state.engineThinkingTime });
		this.gameCode = code;
		await window._connection.invoke("JOIN_GAME", code);
	}

	handleOnGameStarted = () => this.onGameStarted();

	onGameStarted() {
		if (!this.gameCode) return;
		this.setState({ redirect: `/game/${this.gameCode}` });
	}
}
