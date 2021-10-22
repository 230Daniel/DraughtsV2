import React from "react";
import { Redirect } from "react-router";

import styles from "./Play.Module.css";

export default class Play extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			redirect: null
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
				<form className={styles.form} onSubmit={(e) => this.createGame(e)}>
					<span className={styles.title}>Play Draughts</span>
					<button className={styles.button} type="submit">Create Game</button>
				</form>
			</div>
		);
	}

	async createGame(e) {
		e.preventDefault();
		await window._connection.invoke("CREATE_GAME");
		this.setState({ redirect: "/game" });
	}
}
