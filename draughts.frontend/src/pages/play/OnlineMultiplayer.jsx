import React from "react";
import Loader from "react-loader-spinner";
import { Redirect } from "react-router";

import styles from "./Play.Module.css";

export default class OnlineMultiplayer extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			redirect: null,

			joinGameCode: "",
			joinGameCodeLocked: false
		};
	}

	render() {
		if (this.state.redirect) {
			return (
				<Redirect to={this.state.redirect} />
			);
		}

		return (
			<div className={styles.container}>
				<div className={styles.box}>
					<span className={styles.title}>Create a game</span>
					<form onSubmit={(e) => this.onSubmitCreateGame(e)}>
						{!this.state.creatingGame &&
							<button className={styles.button} type="submit">Submit</button>
						}
						{this.state.creatingGame &&
							<Loader className={styles.loader} color="#ffffff" type="ThreeDots" height={30} />
						}
					</form>
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

	async onJoinGameCodeChanged(e) {
		var code = e.target.value.toUpperCase();

		if (code.length === 6) {
			this.setState({ joinGameCode: code, joiningGame: true, invalidGameCode: false });
			var response = await window._connection.invoke("VALIDATE_GAME_CODE", code);
			if (response) {
				this.setState({ redirect: `/game/${code}` });
			} else {
				this.setState({ invalidGameCode: true, joiningGame: false });
			}
		} else {
			this.setState({ joinGameCode: code, invalidGameCode: false });
		}
	}

	async onSubmitCreateGame(e) {
		e.preventDefault();

		this.setState({ creatingGame: true });
		var code = await window._connection.invoke("CREATE_GAME", { gameType: 1 });
		this.setState({ redirect: `/game/${code}` });
	}
}
