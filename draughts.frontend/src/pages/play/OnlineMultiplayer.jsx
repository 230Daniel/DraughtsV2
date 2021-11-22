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
				{this.renderCreateGame()}
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
							<label className={`${styles.codeLabel} ${styles.invalid}`}>Invalid game code</label>
						}
						{this.state.joiningGame &&
							<Loader className={styles.loader} color="#ffffff" type="ThreeDots" height={30} />
						}
					</form>
				</div>
			</div>
		);
	}

	renderCreateGame() {
		if (!this.state.createGameCode) {
			return (
				<div className={styles.box}>
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
				</div>
			);
		}

		return (
			<div className={styles.box}>
				<span className={styles.title}>Waiting for opponent...</span>
				<label className={styles.codeLabel}>Invite a friend using this game code:</label>
				<input type="text" value={this.state.createGameCode} readOnly={true} className={styles.codeInput}></input>
				<button className={styles.button} onClick={async () => {
					this.setState({ createGameCode: "" });
					await window._connection.invoke("LEAVE_GAME", this.gameCode);
					this.gameCode = null;
				}}>Cancel</button>
			</div >
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

		if (code.length === 6) {
			this.setState({ joinGameCode: code, joiningGame: true, invalidGameCode: false });

			if (code == this.state.createGameCode) {
				this.setState({ invalidGameCode: true, joiningGame: false });
				return;
			}

			this.gameCode = code;
			var response = await window._connection.invoke("JOIN_GAME", code);
			if (!response) {
				this.setState({ invalidGameCode: true, joiningGame: false });
			}
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
