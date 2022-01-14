import React from "react";
import Loader from "react-loader-spinner";
import { Redirect } from "react-router";

import styles from "./Play.Module.css";

export default class OnlineMultiplayer extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			redirect: null,

			createGameSide: -1,
			createGameCode: null,

			joinGameCode: "",
			joinGameCodeLocked: false
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
						{this.renderCreateGame()}
					</div>
					<div className={styles.box}>
						<span className={styles.title}>Join a game</span>
						<form>
							<label className={styles.codeLabel}>Enter game code</label>
							<input
								type="text"
								value={this.state.joinGameCode}
								disabled={this.state.joiningGame}
								onChange={(e) => this.onJoinGameCodeChanged(e)}
								maxLength="6"
								placeholder="ABCDEF"
								className={`${styles.codeInput} ${this.state.invalidGameCode ? styles.invalid : null}`}>
							</input>
							{this.state.invalidGameCode &&
								<label className={`${styles.codeLabel} ${styles.invalid}`}>{this.state.createGameCode === this.state.joinGameCode ? "👉( ͡≖ ͜ʖ ͡≖) 👉 nice try kiddo" : "Invalid game code"}</label>
							}
							{this.state.joiningGame &&
								<Loader className={styles.loader} color="#ffffff" type="ThreeDots" height={30} />
							}
						</form>
					</div>
				</div>
			</div>
		);
	}

	renderCreateGame() {
		if (!this.state.createGameCode) {
			return (
				<>
					<span className={styles.title}>Create a game</span>
					<form onSubmit={(e) => this.onSubmitCreateGame(e)}>
						<div className={styles.inputGroup}>
							<label>Play as side</label>
							<select
								value={this.state.createGameSide}
								onChange={(e) => this.setState({ createGameSide: parseInt(e.target.value) })}>
								<option value={-1}>Random</option>
								<option value={0}>Black</option>
								<option value={1}>White</option>
							</select>
						</div>
						{!this.state.creatingGame &&
							<button className={styles.button} type="submit">Create</button>
						}
						{this.state.creatingGame &&
							<Loader className={styles.loader} color="#ffffff" type="ThreeDots" height={30} />
						}
					</form>
				</>
			);
		}

		return (
			<>
				<span className={styles.title}>Waiting for opponent...</span>
				<label className={styles.codeLabel}>Invite a friend using this game code:</label>
				<span className={styles.codeInput}>{this.state.createGameCode}</span>
				<button className={styles.button} onClick={async () => {
					this.setState({ createGameCode: "" });
					await window._connection.invoke("LEAVE_GAME", this.gameCode);
					this.gameCode = null;
				}}>Cancel</button>
			</>
		);
	}

	componentDidMount() {
		window._connection.on("GAME_STARTED", this.handleOnGameStarted);
	}

	componentWillUnmount() {
		// To avoid a memory leak, unregister the game updated event handler
		window._connection.off("GAME_STARTED", this.handleOnGameStarted);
	}

	async onJoinGameCodeChanged(e) {
		var code = e.target.value.toUpperCase();

		// If the code length is 6 the user has finished inputting it
		if (code.length === 6) {

			// Show a loading message
			this.setState({ joinGameCode: code, joiningGame: true, invalidGameCode: false });

			// If this code matches the "Create Game" code reject it to avoid error state
			if (code === this.state.createGameCode) {
				this.setState({ invalidGameCode: true, joiningGame: false });
				return;
			}

			// Send the request to join the game
			this.gameCode = code;
			var response = await window._connection.invoke("JOIN_GAME", code);

			// If the request fails show an error message
			if (!response) {
				this.setState({ invalidGameCode: true, joiningGame: false });
			}

			// If the request succeeded the server will eventually send GAME_STARTED so we're done for now
		} else {
			this.setState({ joinGameCode: code, invalidGameCode: false });
		}
	}

	async onSubmitCreateGame(e) {
		e.preventDefault();

		this.setState({ creatingGame: true });
		var code = await window._connection.invoke("CREATE_GAME", { gameType: 1, side: this.state.createGameSide });
		this.gameCode = code;
		await window._connection.invoke("JOIN_GAME", code);

		this.setState({ creatingGame: false, createGameCode: code });
	}

	handleOnGameStarted = () => this.onGameStarted();

	onGameStarted() {
		if (!this.gameCode) return;
		this.setState({ redirect: `/game/${this.gameCode}` });
	}
}
