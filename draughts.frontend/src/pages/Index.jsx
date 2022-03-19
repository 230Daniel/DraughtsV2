import React from "react";
import { Link } from "react-router-dom";

import styles from "./Index.Module.css";

export default class Index extends React.Component {
	render() {
		return (
			<div className={styles.container}>
				<div className={styles.image} />
				<div className={styles.textContainer}>
					<h1 className={styles.title}>Draughts</h1>
					<h2 className={styles.subtitle}><i>Because Chess would be too hard</i></h2>
					<Link className={styles.link} to="/play">Find a game ➔</Link>
				</div>
			</div>
		);
	}
}
