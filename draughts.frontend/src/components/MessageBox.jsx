import React from "react";

import Loader from "react-loader-spinner";
import { Link } from "react-router-dom";

import styles from "./MessageBox.Module.css";

export default class MessageBox extends React.Component {
	render() {
		return (
			<div className={styles.container}>
				<div className={styles.messageBox}>
					<span className={styles.title}>{this.props.title}</span>
					<span className={styles.message}>{this.props.message}</span>
					{this.props.load &&
						<Loader className={styles.loader} color="#ffffff" type="ThreeDots" height={30} />
					}
					{this.props.link &&
						<Link to={this.props.link}>
							<button className={styles.button}>{this.props.linkLabel}</button>
						</Link>
					}
				</div>
			</div>
		)
	}
}
